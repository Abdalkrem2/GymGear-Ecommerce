namespace Ecommerce.Models.ViewModels
{
    public class SearchResultsVM
    {
        public string Query { get; set; }
        public List<ProductCardVM> Results { get; set; } = new();
    }
}
