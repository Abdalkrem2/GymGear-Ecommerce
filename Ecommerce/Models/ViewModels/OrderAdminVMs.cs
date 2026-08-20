namespace Ecommerce.Models.ViewModels
{
    public class OrderAdminVM
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }
}
