using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Api.Models
{
    /// <summary>
    /// Request model for creating a mock payment URL
    /// </summary>
    public class CreateMockPaymentRequest
    {
        /// <summary>
        /// The ID of the booking this payment is for
        /// </summary>
        [Required]
        public Guid BookingId { get; set; }
        
        /// <summary>
        /// The ID of the user making the payment
        /// </summary>
        [Required]
        public Guid UserId { get; set; }
        
        /// <summary>
        /// The amount to be paid
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Information about the order
        /// </summary>
        [Required]
        public string OrderInfo { get; set; }
        
        /// <summary>
        /// Currency code (default: VND)
        /// </summary>
        public string? Currency { get; set; }
    }
}
