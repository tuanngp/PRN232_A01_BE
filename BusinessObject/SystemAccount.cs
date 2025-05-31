using System.ComponentModel.DataAnnotations;
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
        public string AccountName { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string AccountEmail { get; set; }

        [Required]
        public string AccountPassword { get; set; }

        public AccountRole AccountRole { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<NewsArticle> CreatedNewsArticles { get; set; } =
            new List<NewsArticle>();
        public virtual ICollection<NewsArticle> UpdatedNewsArticles { get; set; } =
            new List<NewsArticle>();
    }
}
