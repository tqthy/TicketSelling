using Common.Messages;
using MassTransit;
using PaymentService.Core.Consumers;
using PaymentService.Core.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InitiatePaymentRequestConsumer>(); // Register your consumer

    // If PaymentService also publishes events (e.g., PaymentSucceeded, PaymentFailed)
    // and you want to use the EF Outbox for that:
    // x.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
    // {
    //     o.QueryDelay = TimeSpan.FromSeconds(10);
    //     o.UsePostgres(); // Or your DB provider
    //     o.UseBusOutbox();
    // });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        var host = rabbitMqConfig.GetValue<string>("Host", "rabbitmq"); // Default to "rabbitmq" for Docker Compose
        var username = rabbitMqConfig.GetValue<string>("Username", "guest");
        var password = rabbitMqConfig.GetValue<string>("Password", "guest");

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Configure the receive endpoint for the InitiatePaymentRequested messages
        // The queue name should be distinct for this consumer.
        // MassTransit will bind it to the exchange named "payment-initiation-requests" (or the default for the message type)
        cfg.ReceiveEndpoint("payment-service-initiate-request-queue", e => // Unique queue name for this consumer
        {
            e.ConfigureConsumer<InitiatePaymentRequestConsumer>(context);

            // Optional: Configure retries, dead-letter, etc. for this endpoint
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
            // e.UseInMemoryOutbox(context); // If not using EF outbox for consumers but want consumer outbox
        });

        // Configure message types published by PaymentService (if any)
        cfg.Message<PaymentSucceeded>(m => m.SetEntityName("payment-succeeded-topic"));
        cfg.Publish<PaymentSucceeded>(p => p.ExchangeType = "fanout");
        // ... other published messages
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();