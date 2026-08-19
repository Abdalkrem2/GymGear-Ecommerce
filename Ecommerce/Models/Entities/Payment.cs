using System.ComponentModel.DataAnnotations.Schema;

namespace GymGear.Web.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string Provider { get; set; } = string.Empty;   // e.g. "Stripe", "PayPal"
        public string Status { get; set; } = "Pending";        // Pending / Paid / Failed
        public string? TransactionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
