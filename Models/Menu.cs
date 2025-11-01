namespace AdminLTEApp.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int Order { get; set; }
        public int? ParentId { get; set; }
        public Menu? Parent { get; set; }
        public ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
        public ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
        public bool IsActive { get; set; } = true;
    }
}