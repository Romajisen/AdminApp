namespace AdminLTEApp.Models
{
    public class RoleMenu
    {
        public string RoleId { get; set; } = string.Empty;
        public int MenuId { get; set; }
        public ApplicationRole Role { get; set; } = new ApplicationRole();
        public Menu Menu { get; set; } = new Menu();
    }
}