using GymGear.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
namespace Ecommerce.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
}
