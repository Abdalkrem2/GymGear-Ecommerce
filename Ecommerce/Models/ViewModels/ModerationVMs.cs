namespace Ecommerce.Models.ViewModels
{
    public class ModerationVM
    {
        public List<PendingCommentVM> PendingComments { get; set; } = new();
        public List<PendingTestimonialVM> PendingTestimonials { get; set; } = new();
    }

    public class PendingCommentVM
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string UserFullName { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PendingTestimonialVM
    {
        public int Id { get; set; }
        public string UserFullName { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
