using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.Data;
using GymGear.Web.Models.Enums;

namespace GymGear.Web.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Processing;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
    }
}
