using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace Services.DTOs
{
    public class CreateNewsArticleDto
    {
        [Required(ErrorMessage = "Tiêu đề bài viết là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string NewsTitle { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Headline không được vượt quá 500 ký tự")]
        public string? Headline { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết là bắt buộc")]
        [MinLength(10, ErrorMessage = "Nội dung bài viết phải có ít nhất 10 ký tự")]
        public string NewsContent { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Nguồn tin không được vượt quá 200 ký tự")]
        public string? NewsSource { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "ID danh mục phải lớn hơn 0")]
        public int CategoryId { get; set; }
    }

    public class UpdateNewsArticleDto
    {
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string? NewsTitle { get; set; }

        [StringLength(500, ErrorMessage = "Headline không được vượt quá 500 ký tự")]
        public string? Headline { get; set; }

        [MinLength(10, ErrorMessage = "Nội dung bài viết phải có ít nhất 10 ký tự")]
        public string? NewsContent { get; set; }

        [StringLength(200, ErrorMessage = "Nguồn tin không được vượt quá 200 ký tự")]
        public string? NewsSource { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID danh mục phải lớn hơn 0")]
        public int? CategoryId { get; set; }
    }

    public class NewsArticleDto
    {
        public int NewsArticleId { get; set; }
        public string NewsTitle { get; set; } = string.Empty;
        public string? Headline { get; set; }
        public string NewsContent { get; set; } = string.Empty;
        public string? NewsSource { get; set; }
        public int CategoryId { get; set; }
        public NewsStatus NewsStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int CreatedById { get; set; }
        public int? UpdatedById { get; set; }
    }

    public class ChangeNewsStatusDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public NewsStatus Status { get; set; }
    }
}