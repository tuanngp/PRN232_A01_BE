using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BusinessObject.Common;

namespace BusinessObject
{
    public class Tag : SoftDeleteEntity
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TagName { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        [JsonIgnore]
        public virtual ICollection<NewsArticleTag> NewsArticleTags { get; set; } =
            new List<NewsArticleTag>();
    }
}
