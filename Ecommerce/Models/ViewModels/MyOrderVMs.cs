namespace Ecommerce.Models.ViewModels
{
    public class MyOrderListVM
    {
        public List<MyOrderVM> Orders { get; set; } = new();
    }

    public class MyOrderVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }

    public class InvoiceVM
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderLineItemVM> Items { get; set; } = new();
    }
}
