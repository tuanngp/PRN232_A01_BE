using System.ComponentModel.DataAnnotations;

namespace Services.DTOs
{
    public class TagDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int ArticleCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateTagDto
    {
        [Required(ErrorMessage = "Tên tag là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên tag không được vượt quá 50 ký tự")]
        public string TagName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Note { get; set; }
    }

    public class UpdateTagDto
    {
        [StringLength(50, ErrorMessage = "Tên tag không được vượt quá 50 ký tự")]
        public string? TagName { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Note { get; set; }
    }

    public class CreateBulkTagsDto
    {
        [Required(ErrorMessage = "Danh sách tên tag là bắt buộc")]
        public List<string> TagNames { get; set; } = new List<string>();

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Note { get; set; }
    }

    public class PopularTagDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }

    public class BulkCreateResultDto
    {
        public List<TagDto> CreatedTags { get; set; } = new List<TagDto>();
        public List<string> DuplicateTagNames { get; set; } = new List<string>();
        public int CreatedCount { get; set; }
        public int DuplicateCount { get; set; }
    }
}