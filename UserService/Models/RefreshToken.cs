using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Data;

namespace UserService.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public bool IsUsed { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

