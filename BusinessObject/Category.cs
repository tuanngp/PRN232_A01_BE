using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Common;

namespace BusinessObject
{
    public class Category : SoftDeleteEntity
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(500)]
        public string CategoryDescription { get; set; }

        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("ParentCategoryId")]
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<NewsArticle> NewsArticles { get; set; } =
            new List<NewsArticle>();
    }
}
