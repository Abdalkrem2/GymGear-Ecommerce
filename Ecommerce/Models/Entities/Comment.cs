using Ecommerce.Data;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
