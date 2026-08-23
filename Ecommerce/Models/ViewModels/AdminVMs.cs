using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Models.ViewModels
{
    public class DashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }

        // Orders chart — last 7 days
        public List<string> OrderChartLabels { get; set; } = new();
        public List<int> OrderChartValues { get; set; } = new();

        // Recent orders table
        public List<OrderAdminVM> RecentOrders { get; set; } = new();
    }

    public class CategoryVM
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public string ImagePath { get; set; }
            = string.Empty;

        public int ProductCount { get; set; }
    }

    public class CategoryFormVM
    {
        public int Id { get; set; }

        [Required(
            ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string Name { get; set; }
            = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }
    }
}