using MediatR;
using BookingService.Application.Features.Events.DTOs;
using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Application.Contracts.Infrastructure;
using BookingService.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Features.Events.Queries
{
    public class GetEventSeatsQueryHandler : IRequestHandler<GetEventSeatsQuery, List<EventSeatDetailDto>>
    {
        private readonly BookingDbContext _dbContext;
        private readonly IEventServiceApiClient _eventServiceApiClient;
        private readonly IVenueServiceApiClient _venueServiceApiClient;
        private readonly ILogger<GetEventSeatsQueryHandler> _logger;

        public GetEventSeatsQueryHandler(
            BookingDbContext dbContext,
            IEventServiceApiClient eventServiceApiClient,
            IVenueServiceApiClient venueServiceApiClient,
            ILogger<GetEventSeatsQueryHandler> logger)
        {
            _dbContext = dbContext;
            _eventServiceApiClient = eventServiceApiClient;
            _venueServiceApiClient = venueServiceApiClient;
            _logger = logger;
        }

        public async Task<List<EventSeatDetailDto>> Handle(GetEventSeatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching available seats for EventId: {EventId}", request.EventId);

            var availableSeatStatuses = await _dbContext.EventSeatStatuses
                .Where(ess => ess.EventId == request.EventId &&
                              (ess.Status == SeatAvailabilityStatus.Available ||
                               (ess.Status == SeatAvailabilityStatus.Reserved && ess.ReservedUntil < DateTime.UtcNow)))
                .AsNoTracking() // Good practice for read-only queries
                .ToListAsync(cancellationToken);

            if (!availableSeatStatuses.Any())
            {
                _logger.LogInformation("No available seats found for EventId: {EventId}", request.EventId);
                return new List<EventSeatDetailDto>();
            }

            var seatIds = availableSeatStatuses.Select(s => s.SeatId).Distinct().ToList();
            var responseDtos = new List<EventSeatDetailDto>();

            // Batch fetch seat details from VenueService
            Dictionary<Guid, SeatDetailsDto> seatDetailsMap = new Dictionary<Guid, SeatDetailsDto>();
            if (seatIds.Any())
            {
                try
                {
                    seatDetailsMap = await _venueServiceApiClient.GetSeatDetailsBatchAsync(seatIds, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch seat details in batch from VenueService for EventId {EventId}", request.EventId);
                    // Decide on error handling: return partial data, throw, or return empty with error indicator?
                    // For now, proceed without seat details if this call fails, but log it.
                }
            }


            foreach (var seatStatus in availableSeatStatuses)
            {
                decimal price = 0;
                try
                {
                    // Fetch price for each seat
                    price = await _eventServiceApiClient.GetSeatPriceAsync(request.EventId, seatStatus.SeatId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get price for SeatId {SeatId} in EventId {EventId}. Price will be 0.", seatStatus.SeatId, request.EventId);
                    // Continue, but price will be default (0)
                }

                seatDetailsMap.TryGetValue(seatStatus.SeatId, out var details);

                responseDtos.Add(new EventSeatDetailDto
                {
                    SeatId = seatStatus.SeatId,
                    Price = price,
                    Status = seatStatus.Status, // Reflect actual current status
                    SeatNumber = details?.SeatNumber,
                    Row = details?.Row,
                    SectionName = details?.SectionName
                });
            }
            _logger.LogInformation("Returning {Count} seat details for EventId: {EventId}", responseDtos.Count, request.EventId);
            return responseDtos;
        }
    }
}