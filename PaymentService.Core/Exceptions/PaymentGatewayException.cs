using System;
using System.Runtime.Serialization;

namespace PaymentService.Core.Exceptions;

[Serializable]
public class PaymentGatewayException : Exception
{
    public string ErrorCode { get; }
    public string GatewayName { get; }
    public bool IsTransient { get; }

    public PaymentGatewayException()
    {
    }

    public PaymentGatewayException(string message) : base(message)
    {
    }

    public PaymentGatewayException(string message, Exception inner) : base(message, inner)
    {
    }

    protected PaymentGatewayException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }

    public PaymentGatewayException(string message, string errorCode, string gatewayName, bool isTransient = false, Exception innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        GatewayName = gatewayName;
        IsTransient = isTransient;
    }
}

[Serializable]
public class PaymentGatewayConfigurationException : PaymentGatewayException
{
    public PaymentGatewayConfigurationException(string message, string gatewayName, string errorCode = "INVALID_CONFIGURATION")
        : base(message, errorCode, gatewayName, false)
    {
    }
}

[Serializable]
public class PaymentGatewayTimeoutException : PaymentGatewayException
{
    public PaymentGatewayTimeoutException(string message, string gatewayName, Exception innerException = null)
        : base(message, "GATEWAY_TIMEOUT", gatewayName, true, innerException)
    {
    }
}

[Serializable]
public class PaymentGatewayValidationException : PaymentGatewayException
{
    public PaymentGatewayValidationException(string message, string gatewayName, string errorCode = "VALIDATION_ERROR")
        : base(message, errorCode, gatewayName, false)
    {
    }
}

[Serializable]
public class PaymentGatewayNotSupportedException : PaymentGatewayException
{
    public PaymentGatewayNotSupportedException(string gatewayName)
        : base($"Payment gateway '{gatewayName}' is not supported.", "GATEWAY_NOT_SUPPORTED", gatewayName, false)
    {
    }
}