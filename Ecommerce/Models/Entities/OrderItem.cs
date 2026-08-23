using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        // Snapshot of the price at purchase time (protects against future price changes)
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public string? Size { get; set; }
    }
}