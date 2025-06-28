using BookingService.Api.Auth;
using BookingService.Api.Middleware;
using BookingService.Application.Contracts.Infrastructure;
using BookingService.Application.Features.Bookings.Commands;
using BookingService.Application.IntegrationEventHandlers;
using BookingService.Domain.AggregatesModel.BookingAggregate;
using BookingService.Infrastructure.Data;
using BookingService.Infrastructure.Repositories;
using Common.Messages;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("GatewayHeaders")
    .AddScheme<AuthenticationSchemeOptions, GatewayHeadersAuthenticationHandler>("GatewayHeaders", options => { });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("BookingDb");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'BookingDb' not found.");
}
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHttpClient<IVenueServiceApiClient, VenueServiceApiClient>(client =>
{
    client.BaseAddress = new Uri("http://venueservice"); 
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddHttpClient<IEventServiceApiClient, EventServiceApiClient>(client =>
{
    client.BaseAddress = new Uri("http://eventservice");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommandHandler).Assembly));


builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IEventSeatStatusRepository, EventSeatStatusRepository>();

builder.Services.AddMassTransit(x =>
{
    // x.AddEntityFrameworkOutbox<BookingDbContext>(o => // If using EF outbox
    // {
    //     o.UsePostgres(); // Or your DB
    //     o.UseBusOutbox();
    // });
    x.AddConsumer<EventApprovedConsumer>();
    x.AddConsumer<PaymentSucceededIntegrationEventHandler>();
    x.AddConsumer<PaymentFailedIntegrationEventHandler>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        var host = rabbitMqConfig.GetValue<string>("Host", "rabbitmq");
        var username = rabbitMqConfig.GetValue<string>("Username", "guest"); 
        var password = rabbitMqConfig.GetValue<string>("Password", "guest");
        
        cfg.Host(host, "/", h => 
        {
            h.Username(username!);
            h.Password(password!);
        });
        
        cfg.Message<EventApproved>(m => m.SetEntityName("event-approved"));
        
        cfg.ReceiveEndpoint("booking-service.event", e => // Kebab-case queue name
        {
            e.ClearSerialization();
            e.UseRawJsonSerializer();
            e.ConfigureConsumer<EventApprovedConsumer>(context);
            
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ));
            
            e.UseInMemoryOutbox(context); 
        });
        
        cfg.ReceiveEndpoint("booking-payment-results", e =>
        {
            // Bind the consumers to this endpoint
            e.ConfigureConsumer<PaymentSucceededIntegrationEventHandler>(context);
            e.ConfigureConsumer<PaymentFailedIntegrationEventHandler>(context);
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BookingDbContext>();
    await BookingDbContextSeed.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<ClaimsFromHeadersMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();