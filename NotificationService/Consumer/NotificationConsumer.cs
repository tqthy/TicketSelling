namespace NotificationService.Consumer;

using MassTransit;
using Common.Messages; // Your message contract
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration; // For email settings
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class NotificationConsumer : IConsumer<EmailNotificationRequested>
{
    
        private readonly ILogger<NotificationConsumer> _logger;
    private readonly IConfiguration _configuration;

    public NotificationConsumer(ILogger<NotificationConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<EmailNotificationRequested> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received EmailNotificationRequested for: {ToEmail}, Subject: {Subject}", message.ToEmail, message.Subject);

        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("TicketSelling", emailSettings["SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", message.ToEmail));
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder();
            if (message.IsHtmlBody)
            {
                bodyBuilder.HtmlBody = message.Body;
            }
            else
            {
                bodyBuilder.TextBody = message.Body;
            }
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // For demo purposes, accept all SSL certificates (not recommended for production)
                // client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Or configure proper SSL

                await client.ConnectAsync(emailSettings["SmtpHost"], int.Parse(emailSettings["SmtpPort"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation("Email sent successfully to {ToEmail}", message.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for notification to {ToEmail}", message.ToEmail);
            // Depending on your error handling strategy, you might want to re-throw
            // to let MassTransit handle retries or move to an error queue.
            throw;
        }
    }
    
}