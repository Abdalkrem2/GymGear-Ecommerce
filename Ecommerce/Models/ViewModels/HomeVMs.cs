using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.ViewModels
{
    public class HomeVM
    {
        public List<CategoryNavVM> Categories { get; set; } = new();
        public List<ProductCardVM> NewArrivals { get; set; } = new();
        public List<ProductCardVM> MostFavorites { get; set; } = new();
        public List<ProductCardVM> WomensCollection { get; set; } = new();
        public List<ProductCardVM> MensCollection { get; set; } = new();
    }

    public class CategoryNavVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AboutVM
    {
        public List<TestimonialVM> Testimonials { get; set; } = new();
        public TestimonialSubmitVM NewTestimonial { get; set; } = new();
    }

    public class TestimonialVM
    {
        public string UserFullName { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TestimonialSubmitVM
    {
        [Required, StringLength(1000)]
        public string Text { get; set; }
    }
}
