namespace UserService.Models
{
    public class PasswordResetRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordResetDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

