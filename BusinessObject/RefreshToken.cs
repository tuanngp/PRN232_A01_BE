using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Common;

namespace BusinessObject
{
    public class RefreshToken : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;
        
        public bool IsRevoked { get; set; } = false;
        
        public DateTime? RevokedDate { get; set; }

        public string? ReasonRevoked { get; set; }

        public string? ReplacedByToken { get; set; }

        [Required]
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public virtual SystemAccount Account { get; set; } = null!;
        
        public bool IsActive => !IsRevoked && !IsUsed && ExpiryDate > DateTime.UtcNow;
    }
} 