using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminLTEApp.Data;
using AdminLTEApp.Models;

namespace AdminLTEApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsActive = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false; // Soft delete
                var result = await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        // Add method to assign menus to user
        public async Task<IActionResult> AssignMenus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allMenus = await _context.Menus.ToListAsync();
            var assignedMenuIds = await _context.UserMenus
                .Where(um => um.UserId == id)
                .Select(um => um.MenuId)
                .ToListAsync();

            var model = new UserMenuAssignmentViewModel
            {
                UserId = id,
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
        public async Task<IActionResult> AssignMenus(UserMenuAssignmentViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var existingUserMenus = await _context.UserMenus
                .Where(um => um.UserId == model.UserId)
                .ToListAsync();

            var selectedMenuIds = model.Menus.Where(m => m.IsChecked).Select(m => m.Id).ToList();

            // Remove assignments that are no longer selected
            var menusToRemove = existingUserMenus.Where(um => !selectedMenuIds.Contains(um.MenuId));
            _context.UserMenus.RemoveRange(menusToRemove);

            // Add new assignments
            var existingMenuIds = existingUserMenus.Select(um => um.MenuId).ToList();
            var menusToAdd = selectedMenuIds.Where(id => !existingMenuIds.Contains(id))
                .Select(id => new UserMenu { UserId = model.UserId, MenuId = id });
            _context.UserMenus.AddRange(menusToAdd);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AssignMenus), new { id = model.UserId });
        }
    }
}