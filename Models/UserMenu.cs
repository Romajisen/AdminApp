namespace AdminLTEApp.Models
{
    public class UserMenu
    {
        public string UserId { get; set; } = string.Empty;
        public int MenuId { get; set; }
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public Menu Menu { get; set; } = new Menu();
    }
}