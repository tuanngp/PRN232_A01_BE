using System.ComponentModel.DataAnnotations;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs
{
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
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 tag")]
        public List<string> TagNames { get; set; } = new List<string>();

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Note { get; set; }
    }

    public class TagStatisticsDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
    }

    public class PopularTagDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
}
