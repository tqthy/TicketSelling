using System.Runtime.Serialization;

namespace PaymentService.Core.Exceptions;

[Serializable]
public class PaymentProcessingException : Exception
{
    public string ErrorCode { get; }
    public bool IsTransient { get; }

    public PaymentProcessingException()
    {
    }

    public PaymentProcessingException(string message) : base(message)
    {
    }

    public PaymentProcessingException(string message, Exception inner) : base(message, inner)
    {
    }

    protected PaymentProcessingException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }

    public PaymentProcessingException(
        string message, 
        string errorCode, 
        bool isTransient = false, 
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        IsTransient = isTransient;
    }
}

[Serializable]
public class WebhookProcessingException : PaymentProcessingException
{
    public WebhookProcessingException(
        string message, 
        string errorCode, 
        bool isTransient = false, 
        Exception? innerException = null)
        : base(message, errorCode, isTransient, innerException)
    {
    }

    protected WebhookProcessingException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
