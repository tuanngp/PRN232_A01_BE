using System.ComponentModel.DataAnnotations;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả danh mục không được vượt quá 500 ký tự")]
        public string CategoryDescription { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "ID danh mục cha phải lớn hơn 0")]
        public int? ParentCategoryId { get; set; }
    }

    public class UpdateCategoryDto
    {
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string? CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả danh mục không được vượt quá 500 ký tự")]
        public string? CategoryDescription { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ID danh mục cha phải lớn hơn 0")]
        public int? ParentCategoryId { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CategoryTreeDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = new List<CategoryTreeDto>();
    }
}
