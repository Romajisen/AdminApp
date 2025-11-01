using Microsoft.AspNetCore.Identity;

namespace AdminLTEApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }    = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<UserMenu> UsersMenus { get; set; } = new List<UserMenu>();
    }
}