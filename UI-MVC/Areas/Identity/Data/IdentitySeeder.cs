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
        var test2 = new IdentityUser { UserName = "test2@test.tld", Email = "test2@test.tld"};
        var test3 = new IdentityUser { UserName = "test3@test.tld", Email = "test3@test.tld"};
        var test4 = new IdentityUser { UserName = "test4@test.tld", Email = "test4@test.tld"};
        
        userManager.CreateAsync(admin, "Admin@123$").Wait();
        userManager.CreateAsync(test1, "Test1@123$").Wait();
        userManager.CreateAsync(test2, "Test2@123$").Wait();
        userManager.CreateAsync(test3, "Test3@123$").Wait();
        userManager.CreateAsync(test4, "Test4@123$").Wait();
        
        userManager.AddToRoleAsync(admin, "Admin").Wait();
        userManager.AddToRoleAsync(test1, "User").Wait();
        userManager.AddToRoleAsync(test2, "User").Wait();
        userManager.AddToRoleAsync(test3, "User").Wait();
        userManager.AddToRoleAsync(test4, "User").Wait();
    }
}