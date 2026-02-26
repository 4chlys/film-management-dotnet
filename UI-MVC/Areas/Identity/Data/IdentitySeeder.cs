using FilmManagement.DAL.EF;
using Microsoft.AspNetCore.Identity;

namespace FilmManagement.UI.Web.Areas.Identity.Data;

public class IdentitySeeder(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public void Seed()
    {
        var adminRole = new IdentityRole { Name = nameof(UserRoleName.Admin) };
        var userRole = new IdentityRole { Name = nameof(UserRoleName.User) };
        
        roleManager.CreateAsync(adminRole).Wait();
        roleManager.CreateAsync(userRole).Wait();
        
        var admin = new IdentityUser { UserName = "admin@test.tld", Email = "admin@test.tld"};
        var test1 = new IdentityUser { UserName = "test1@test.tld", Email = "test1@test.tld"};
        
        userManager.CreateAsync(admin, "Admin123$").Wait();
        userManager.CreateAsync(test1, "Test123$").Wait();
        userManager.AddToRoleAsync(admin, "Admin").Wait();
        userManager.AddToRoleAsync(test1, "User").Wait();
    }
}