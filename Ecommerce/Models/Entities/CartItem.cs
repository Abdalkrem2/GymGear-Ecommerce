using Ecommerce.Data;

namespace Ecommerce.Models.Entities
{
    // Supports both guest carts (via SessionId) and logged-in users (via UserId).
    // On login, merge SessionId rows into the user's rows (increment quantity, no duplicates).
    public class CartItem
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? SessionId { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
