namespace Ecommerce.Models.ViewModels
{
    public class UserListItemVM
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? JoinDate { get; set; }
        public int OrderCount { get; set; }
    }
}
