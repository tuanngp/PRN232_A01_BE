namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs
{
    public class AddTagToArticleDto
    {
        public int TagId { get; set; }
    }

    public class AddMultipleTagsDto
    {
        public List<int> TagIds { get; set; } = new List<int>();
    }

    public class ReplaceTagsDto
    {
        public List<int>? TagIds { get; set; } = new List<int>();
    }
}
