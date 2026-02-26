using FilmManagement.DAL.EF;
using Microsoft.AspNetCore.Identity;

namespace FilmManagement.UI.Web.Areas.Identity.Data;

public class IdentitySeeder(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public void Seed()
    {
        var adminRole = new IdentityRole { Name = nameof(AdminRoleName.Admin) };
        
        roleManager.CreateAsync(adminRole).Wait();
        
        var admin = new IdentityUser { UserName = "admin", Email = "" };
        
        userManager.CreateAsync(admin, "Admin123$").Wait();
        userManager.AddToRoleAsync(admin, "Admin").Wait();
    }
}