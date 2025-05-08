using MassTransit;
using Common.Messages; // For EventApprovedIntegrationEvent
using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Infrastructure.Data; // For BookingDbContext
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore; // For List

namespace BookingService.Application.IntegrationEventHandlers
{
    public class EventApprovedConsumer : IConsumer<EventApproved>
    {
        private readonly BookingDbContext _dbContext; 
        private readonly ILogger<EventApprovedConsumer> _logger;

        public EventApprovedConsumer(
            BookingDbContext dbContext,
            ILogger<EventApprovedConsumer> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<EventApproved> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "Received EventApprovedIntegrationEvent for EventId: {EventId} with {SeatCount} seats",
                message.EventId, message.SeatIds.Count);

            // Idempotency check: Ensure we haven't processed this event's seats already.
            // This can be complex. A simple check might be to see if any seat status for this event already exists.
            // A more robust check might involve storing processed event IDs or using a distributed lock if high concurrency is expected.
            bool alreadyProcessed = await _dbContext.EventSeatStatuses
                                           .AnyAsync(ess => ess.EventId == message.EventId && message.SeatIds.Contains(ess.SeatId),
                                                     context.CancellationToken);

            if (alreadyProcessed)
            {
                _logger.LogWarning("Event {EventId} seat statuses might have been already processed. Skipping creation for provided seats.", message.EventId);
                // Depending on strictness, you might only add non-existing ones or log and return.
                // For this example, we'll create only if no matching seat for the event exists.
                // A more fine-grained check per seat might be needed for partial processing.
            }

            var newSeatStatuses = new List<EventSeatStatus>();
            foreach (var seatId in message.SeatIds)
            {
                 // More precise idempotency: check if this specific seat for this event already exists
                 var existingStatus = await _dbContext.EventSeatStatuses
                    .FirstOrDefaultAsync(s => s.EventId == message.EventId && s.SeatId == seatId, context.CancellationToken);

                 if (existingStatus == null)
                 {
                    newSeatStatuses.Add(new EventSeatStatus(message.EventId, seatId)); // Status defaults to 'Available'
                 }
                 else
                 {
                    _logger.LogInformation("Seat status for Event {EventId}, Seat {SeatId} already exists with status {Status}. Skipping.",
                                           message.EventId, seatId, existingStatus.Status);
                 }
            }

            if (newSeatStatuses.Any())
            {
                try
                {
                    // Using DbContext directly:
                    await _dbContext.EventSeatStatuses.AddRangeAsync(newSeatStatuses, context.CancellationToken);
                    await _dbContext.SaveChangesAsync(context.CancellationToken);

                    // Or using repository if AddRangeAsync is implemented:
                    // await _seatStatusRepository.AddRangeAsync(newSeatStatuses);
                    // await _dbContext.SaveChangesAsync(context.CancellationToken); // Assuming UoW is dbContext

                    _logger.LogInformation(
                        "Successfully created {Count} 'Available' EventSeatStatus records for EventId: {EventId}",
                        newSeatStatuses.Count, message.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating EventSeatStatus records for EventId: {EventId}", message.EventId);
                    // Rethrow to allow MassTransit to handle retries/error queue
                    throw;
                }
            }
            else
            {
                _logger.LogInformation("No new seat statuses to create for EventId: {EventId} (all provided seats might already exist).", message.EventId);
            }
        }
    }
}