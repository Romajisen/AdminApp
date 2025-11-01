using AdminLTEApp.Data;
using AdminLTEApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminLTEApp.Views.Shared.Components.Menu
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MenuViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(UserClaimsPrincipal);
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            var userRoles = await _userManager.GetRolesAsync(user);

            var allowedMenuIds = new List<int>();

            // Ambil semua RoleMenus dan Roles terlebih dahulu
            var roleMenus = await _context.RoleMenus.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            // Filter berdasarkan nama role di memory
            var roleMenuIds = roleMenus
                .Where(rm => userRoles.Contains(roles.FirstOrDefault(r => r.Id == rm.RoleId)?.Name ?? ""))
                .Select(rm => rm.MenuId)
                .ToList();

            // Get menus assigned directly to user
            var userMenuIds = await _context.UserMenus
                .Where(um => um.UserId == userId)
                .Select(um => um.MenuId)
                .ToListAsync();

            allowedMenuIds.AddRange(roleMenuIds);
            allowedMenuIds.AddRange(userMenuIds);
            allowedMenuIds = allowedMenuIds.Distinct().ToList();

            var menus = await _context.Menus
                .Where(m => m.IsActive && allowedMenuIds.Contains(m.Id))
                .Where(m => m.ParentId == null)
                .Include(m => m.Children.Where(c => c.IsActive && allowedMenuIds.Contains(c.Id)))
                .OrderBy(m => m.Order)
                .ToListAsync();

            return View(menus);
        }
    }
}