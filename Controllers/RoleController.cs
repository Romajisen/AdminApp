using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminLTEApp.Data;
using AdminLTEApp.Models;

namespace AdminLTEApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleController(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new ApplicationRole
                {
                    Name = model.Name,
                    Description = model.Description
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null) return NotFound();

                role.Name = model.Name;
                role.Description = model.Description;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
            }

            return RedirectToAction(nameof(Index));
        }

        // Add method to assign menus to role
        public async Task<IActionResult> AssignMenus(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var allMenus = await _context.Menus.ToListAsync();
            var assignedMenuIds = await _context.RoleMenus
                .Where(rm => rm.RoleId == id)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            var model = new RoleMenuAssignmentViewModel
            {
                RoleId = id,
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
        public async Task<IActionResult> AssignMenus(RoleMenuAssignmentViewModel model)
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
            return RedirectToAction(nameof(AssignMenus), new { id = model.RoleId });
        }
    }
}