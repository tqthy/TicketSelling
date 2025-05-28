// // using Swashbuckle.AspNetCore.Annotations;
//
// namespace PaymentService.Api.DTOs;
//
//
// public class GetVnPayUrlDto
// {
//     
//     // [SwaggerSchema(Description = "Unique identifier for the booking")]
//     public Guid BookingId { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Unique identifier for the user")]
//     public Guid UserId { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Type of the order")]
//     public string OrderType { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Amount to be paid")]
//     public decimal Amount { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Information about the order")]
//     public string OrderInfo { get; set; }
//
//     
//     // [SwaggerSchema(Description = "IP address of the user")]
//     public string IpAddress { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Currency for the payment")]
//     public string Currency { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Date when the payment was created")]
//     public DateTime CreateDate { get; set; }
//
//     
//     // [SwaggerSchema(Description = "Expiration date for the payment")]
//     public DateTime ExpireDate { get; set; }
// }




// using Swashbuckle.AspNetCore.Annotations;
//
namespace PaymentService.Api.DTOs;

/// <summary>
/// DTO for VNPay URL request
/// </summary>
public class GetVnPayUrlDto
{
    /// <example>d3b07384-d9a7-4f3b-8a9d-0e4e5f3b2f3c</example>
    // [SwaggerSchema(Description = "Unique identifier for the booking")]
    public Guid BookingId { get; set; }

    /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
    // [SwaggerSchema(Description = "Unique identifier for the user")]
    public Guid UserId { get; set; }

    /// <example>Online</example>
    // [SwaggerSchema(Description = "Type of the order")]
    public string OrderType { get; set; }

    /// <example>100000</example>
    // [SwaggerSchema(Description = "Amount to be paid")]
    public decimal Amount { get; set; }

    /// <example>Payment for booking #12345</example>
    // [SwaggerSchema(Description = "Information about the order")]
    public string OrderInfo { get; set; }

    /// <example>192.168.1.1</example>
    // [SwaggerSchema(Description = "IP address of the user")]
    public string IpAddress { get; set; }

    /// <example>VND</example>
    // [SwaggerSchema(Description = "Currency for the payment")]
    public string Currency { get; set; }

    /// <example>2023-10-01T12:00:00</example>
    // [SwaggerSchema(Description = "Date when the payment was created")]
    public DateTime CreateDate { get; set; }

    /// <example>2023-10-02T12:10:00</example>
    // [SwaggerSchema(Description = "Expiration date for the payment")]
    public DateTime ExpireDate { get; set; }
}

