namespace Ecommerce.Models.ViewModels
{
    public class ProductListVM
    {
        public List<ProductCardVM> Products { get; set; } = new();
        public List<CategoryNavVM> Categories { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class ProductCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string MainImagePath { get; set; }
    }

    public class ProductDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; }
        public List<ProductImageVM> Images { get; set; } = new();
    }
}
