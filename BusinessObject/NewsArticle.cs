using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Common;
using BusinessObject.Enums;

namespace BusinessObject
{
    public class NewsArticle : SoftDeleteEntity
    {
        [Key]
        public int NewsArticleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string NewsTitle { get; set; }

        [MaxLength(500)]
        public string? Headline { get; set; }

        [Required]
        public string NewsContent { get; set; }

        [MaxLength(200)]
        public string? NewsSource { get; set; }

        public DateTime CreatedDate { get; set; }

        public NewsStatus NewsStatus { get; set; }

        public int CategoryId { get; set; }
        public int CreatedById { get; set; }
        public int? UpdatedById { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("CreatedById")]
        public virtual SystemAccount? CreatedBy { get; set; }

        [ForeignKey("UpdatedById")]
        public virtual SystemAccount? UpdatedBy { get; set; }

        public virtual ICollection<NewsArticleTag> NewsArticleTags { get; set; } =
            new List<NewsArticleTag>();
    }
}
