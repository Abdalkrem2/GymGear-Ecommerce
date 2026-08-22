using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [NotMapped]
        public double AverageRating => Comments.Any(c => c.IsApproved)
            ? Comments.Where(c => c.IsApproved).Average(c => c.Rating)
            : 0;
    }
}
