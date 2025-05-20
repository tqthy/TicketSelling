namespace PaymentService.Core.Exceptions;

public class PaymentStateException : Exception
{
    public PaymentStateException() { }
    public PaymentStateException(string message) : base(message) { }
    public PaymentStateException(string message, Exception inner) : base(message, inner) { }
}