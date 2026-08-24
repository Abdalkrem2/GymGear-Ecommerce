using System.ComponentModel.DataAnnotations;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Models.ViewModels
{
    public class ProductAdminVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool HasSizes { get; set; }
        public bool IsActive { get; set; }
        public string MainImagePath { get; set; }
    }

    public class ProductFormVM
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required, Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool HasSizes { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<CategoryNavVM> Categories { get; set; } = new();

        public List<IFormFile> ImageFiles { get; set; } = new();

        public List<ProductImageVM> ExistingImages { get; set; } = new();
    }

    public class ProductImageVM
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsMain { get; set; }
    }
}