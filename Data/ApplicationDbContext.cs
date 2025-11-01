using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AdminLTEApp.Models;

namespace AdminLTEApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<UserMenu> UserMenus { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(400);
            });

            // Configure Menu relationships
            builder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure RoleMenu
            builder.Entity<RoleMenu>()
                .HasKey(rm => new { rm.RoleId, rm.MenuId });

            builder.Entity<RoleMenu>()
                .HasOne(rm => rm.Role)
                .WithMany(r => r.RolesMenus)
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoleMenu>()
                .HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserMenu
            builder.Entity<UserMenu>()
                .HasKey(um => new { um.UserId, um.MenuId });

            builder.Entity<UserMenu>()
                .HasOne(um => um.User)
                .WithMany(u => u.UsersMenus)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserMenu>()
                .HasOne(um => um.Menu)
                .WithMany(m => m.UserMenus)
                .HasForeignKey(um => um.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default menus
            SeedDefaultMenus(builder);
        }

        private void SeedDefaultMenus(ModelBuilder builder)
        {
            builder.Entity<Menu>().HasData(
                new Menu { Id = 1, Name = "Dashboard", Controller = "Home", Action = "Index", Icon = "fas fa-tachometer-alt", Order = 1, IsActive = true },
                new Menu { Id = 2, Name = "User Management", Controller = "User", Action = "Index", Icon = "fas fa-users", Order = 2, IsActive = true },
                new Menu { Id = 3, Name = "Role Management", Controller = "Role", Action = "Index", Icon = "fas fa-shield-alt", Order = 3, IsActive = true },
                new Menu { Id = 4, Name = "Menu Management", Controller = "Menu", Action = "Index", Icon = "fas fa-list", Order = 4, IsActive = true }
            );
        }
    }
}