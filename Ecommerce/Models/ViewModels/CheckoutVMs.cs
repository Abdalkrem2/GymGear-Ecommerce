using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models.ViewModels
{
    public class CheckoutVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal Subtotal { get; set; }

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(200)]
        public string StreetAddress { get; set; }

        [Required, CreditCard] 
        public string CardNumber { get; set; }

        [Required, StringLength(5)] 
        public string ExpiryDate { get; set; }

        [Required, StringLength(4)] 
        public string CVC { get; set; }

        [Required, StringLength(100)]
        public string City { get; set; }

        [Required, StringLength(100)]
        public string State { get; set; }

        [Required, StringLength(20)]
        public string ZipCode { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }
    }

    public class OrderConfirmationVM
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
    }
}
