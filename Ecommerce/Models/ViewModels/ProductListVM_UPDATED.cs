namespace Ecommerce.Models.ViewModels
{
    // UPDATED in Phase 4 — added MinPrice, MaxPrice, SortBy for advanced filtering.
    // This REPLACES the Phase 2 version of ProductListVM in ProductStorefrontVMs.cs — Hanen/Belal:
    // update the existing class there rather than creating a duplicate file.
    public class ProductListVM
    {
        public List<ProductCardVM> Products { get; set; } = new();
        public List<CategoryNavVM> Categories { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        // New in Phase 4
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; }
    }
}
