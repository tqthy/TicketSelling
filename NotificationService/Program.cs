using MailKit.Net.Smtp;
using MailKit.Security;
using MassTransit;
using MimeKit;
using NotificationService.Consumer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    // Register your consumer
    x.AddConsumer<NotificationConsumer>(); // You'll create this consumer

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ");
        var host = rabbitMqConfig.GetValue<string>("Host", "rabbitmq");
        var username = rabbitMqConfig.GetValue<string>("Username", "guest");
        var password = rabbitMqConfig.GetValue<string>("Password", "guest");

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Configure the receive endpoint
        // "notification-service-queue" should be the name of the queue
        // that other services will send notification messages to.
        cfg.ReceiveEndpoint("notification-service-email-queue", e =>
        {
            e.ConfigureConsumer<NotificationConsumer>(context);
            
            // Bind to the exchange explicitly if not using default fanout behavior for the message type
            // For fanout exchanges, consumers typically declare their own queue and bind it.
            // If "email-notifications-exchange" is a fanout exchange, MassTransit handles this.
            // If it's direct/topic, you might need more specific binding configurations,
            // but for fanout, just configuring the endpoint to consume EmailNotificationRequested is usually enough.
            
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
        });
    });
});

builder.Services.AddScoped<NotificationConsumer>();

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


