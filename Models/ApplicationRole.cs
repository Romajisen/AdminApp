using Microsoft.AspNetCore.Identity;

namespace AdminLTEApp.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; } = string.Empty;
        public ICollection<RoleMenu> RolesMenus { get; set; } = new List<RoleMenu>();
    }
}