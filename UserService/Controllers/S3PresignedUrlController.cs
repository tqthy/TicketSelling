namespace UserService.Controllers;

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class S3PresignedUrlController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration; // To read bucket name if stored in config

    public S3PresignedUrlController(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
    }

    [HttpGet("generate-download-url")]
    public IActionResult GenerateDownloadUrl(string objectKey, double durationInHours = 1)
    {
        string bucketName = _configuration["S3:BucketName"]; // Assuming bucket name is stored in appsettings.json
        if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(objectKey))
        {
            return BadRequest("Bucket name and object key are required.");
        }

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Verb = HttpVerb.GET, // For download
                Expires = DateTime.UtcNow.AddHours(durationInHours)
            };

            string url = _s3Client.GetPreSignedURL(request);
            return Ok(new { PresignedUrl = url });
        }
        catch (AmazonS3Exception e)
        {
            // Log the exception
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error generating presigned URL: {e.Message}");
        }
        catch (Exception e)
        {
            // Log the exception
            return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {e.Message}");
        }
    }

    [HttpGet("generate-upload-url")]
    public IActionResult GenerateUploadUrl(string objectKey, double durationInHours = 1, string contentType = "application/octet-stream")
    {
        string bucketName = _configuration["AWS:BucketName"]; 
        if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(objectKey))
        {
            return BadRequest("Bucket name and object key are required.");
        }

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT, // For upload
                Expires = DateTime.UtcNow.AddHours(durationInHours),
                ContentType = contentType // Important for uploads
            };

            string url = _s3Client.GetPreSignedURL(request);
            return Ok(new { PresignedUrl = url });
        }
        catch (AmazonS3Exception e)
        {
            // Log the exception
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error generating presigned URL: {e.Message}");
        }
        catch (Exception e)
        {
            // Log the exception
            return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred: {e.Message}");
        }
    }
}