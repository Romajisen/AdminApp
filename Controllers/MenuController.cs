using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminLTEApp.Data;
using AdminLTEApp.Models;

namespace AdminLTEApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public MenuController(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var menus = await _context.Menus
                .Where(m => m.ParentId == null)
                .Include(m => m.Children)
                .OrderBy(m => m.Order)
                .ToListAsync();
            return View(menus);
        }

        public async Task<IActionResult> Create()
        {
            var parentMenus = await _context.Menus
                .Where(m => m.ParentId == null)
                .ToListAsync();
            ViewBag.ParentMenus = parentMenus;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Menu model)
        {
            if (ModelState.IsValid)
            {
                _context.Menus.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var parentMenus = await _context.Menus
                .Where(m => m.ParentId == null)
                .ToListAsync();
            ViewBag.ParentMenus = parentMenus;
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound();

            var parentMenus = await _context.Menus
                .Where(m => m.ParentId == null && m.Id != id)
                .ToListAsync();
            ViewBag.ParentMenus = parentMenus;
            return View(menu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Menu model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Menus.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuExists(model.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var parentMenus = await _context.Menus
                .Where(m => m.ParentId == null && m.Id != id)
                .ToListAsync();
            ViewBag.ParentMenus = parentMenus;
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Role Menu Assignment
        public async Task<IActionResult> AssignRoleMenu(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var allMenus = await _context.Menus.ToListAsync();
            var assignedMenuIds = await _context.RoleMenus
                .Where(rm => rm.RoleId == roleId)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            var model = new RoleMenuAssignmentViewModel
            {
                RoleId = roleId,
                RoleName = role.Name ?? string.Empty,
                Menus = allMenus.Select(m => new MenuCheckboxViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    IsChecked = assignedMenuIds.Contains(m.Id),
                    Controller = m.Controller,
                    Action = m.Action
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoleMenu(RoleMenuAssignmentViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return NotFound();

            // Remove existing role-menu assignments
            var existingRoleMenus = _context.RoleMenus.Where(rm => rm.RoleId == model.RoleId);
            _context.RoleMenus.RemoveRange(existingRoleMenus);

            // Add new assignments
            foreach (var menuId in model.Menus.Where(m => m.IsChecked).Select(m => m.Id))
            {
                _context.RoleMenus.Add(new RoleMenu
                {
                    RoleId = model.RoleId,
                    MenuId = menuId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AssignRoleMenu), new { roleId = model.RoleId });
        }

        // User Menu Assignment
        public async Task<IActionResult> AssignUserMenu(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var allMenus = await _context.Menus.ToListAsync();
            var assignedMenuIds = await _context.UserMenus
                .Where(um => um.UserId == userId)
                .Select(um => um.MenuId)
                .ToListAsync();

            var model = new UserMenuAssignmentViewModel
            {
                UserId = userId,
                UserName = user.UserName ?? string.Empty,
                Menus = allMenus.Select(m => new MenuCheckboxViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    IsChecked = assignedMenuIds.Contains(m.Id),
                    Controller = m.Controller,
                    Action = m.Action
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUserMenu(UserMenuAssignmentViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            // Remove existing user-menu assignments
            var existingUserMenus = _context.UserMenus.Where(um => um.UserId == model.UserId);
            _context.UserMenus.RemoveRange(existingUserMenus);

            // Add new assignments
            foreach (var menuId in model.Menus.Where(m => m.IsChecked).Select(m => m.Id))
            {
                _context.UserMenus.Add(new UserMenu
                {
                    UserId = model.UserId,
                    MenuId = menuId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AssignUserMenu), new { userId = model.UserId });
        }

        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.Id == id);
        }
    }
}