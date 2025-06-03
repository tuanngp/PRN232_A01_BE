using System.ComponentModel.DataAnnotations;

namespace Services.DTOs
{
    public class NewsArticleTagDto
    {
        public int NewsArticleId { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
    }

    public class AddTagToArticleDto
    {
        [Required(ErrorMessage = "ID tag là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "ID tag phải lớn hơn 0")]
        public int TagId { get; set; }
    }

    public class AddMultipleTagsDto
    {
        [Required(ErrorMessage = "Danh sách tag là bắt buộc")]
        public List<int> TagIds { get; set; } = new List<int>();
    }

    public class ReplaceTagsDto
    {
        [Required(ErrorMessage = "Danh sách tag là bắt buộc")]
        public List<int> TagIds { get; set; } = new List<int>();
    }

    public class TagStatisticDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
    }

    public class ArticleTagSummaryDto
    {
        public int NewsArticleId { get; set; }
        public string NewsTitle { get; set; } = string.Empty;
        public List<NewsArticleTagDto> Tags { get; set; } = new List<NewsArticleTagDto>();
    }
}