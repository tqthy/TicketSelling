namespace Common.Messages;

public record EmailNotificationRequested
(
    string ToEmail,
    string Subject,
    string Body,
    bool IsHtmlBody // Optional: to specify if the body is HTML
);