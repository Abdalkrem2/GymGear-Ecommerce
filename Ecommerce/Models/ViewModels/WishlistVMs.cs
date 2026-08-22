namespace Ecommerce.Models.ViewModels
{
    public class WishlistVM
    {
        public List<WishlistItemVM> Items { get; set; } = new();
    }

    public class WishlistItemVM
    {
        public int WishlistItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public string MainImagePath { get; set; }
    }
}
