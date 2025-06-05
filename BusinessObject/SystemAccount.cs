using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BusinessObject.Common;
using BusinessObject.Enums;

namespace BusinessObject
{
    public class SystemAccount : AuditableEntity
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string AccountEmail { get; set; } = string.Empty;

        [Required]
        public string AccountPassword { get; set; } = string.Empty;

        public AccountRole AccountRole { get; set; }
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public virtual ICollection<NewsArticle> CreatedNewsArticles { get; set; } =
            new List<NewsArticle>();

        [JsonIgnore]
        public virtual ICollection<NewsArticle> UpdatedNewsArticles { get; set; } =
            new List<NewsArticle>();
    }
}
