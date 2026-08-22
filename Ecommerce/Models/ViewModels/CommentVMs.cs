using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.ViewModels
{
    public class CommentSubmitVM
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(1000)]
        public string Text { get; set; }
    }

    public class CommentVM
    {
        public string UserFullName { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
