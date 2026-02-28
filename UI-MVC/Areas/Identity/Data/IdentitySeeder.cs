using Microsoft.AspNetCore.Identity;

namespace FilmManagement.UI.Web.Areas.Identity.Data;

public class IdentitySeeder(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync()
    {
        await roleManager.CreateAsync(new IdentityRole { Name = nameof(UserRoleName.Admin) });
        await roleManager.CreateAsync(new IdentityRole { Name = nameof(UserRoleName.User) });

        var admin = new IdentityUser { UserName = "admin@test.tld", Email = "admin@test.tld" };
        var test1 = new IdentityUser { UserName = "test1@test.tld", Email = "test1@test.tld" };
        var test2 = new IdentityUser { UserName = "test2@test.tld", Email = "test2@test.tld" };
        var test3 = new IdentityUser { UserName = "test3@test.tld", Email = "test3@test.tld" };
        var test4 = new IdentityUser { UserName = "test4@test.tld", Email = "test4@test.tld" };

        await userManager.CreateAsync(admin, "Admin@123$");
        await userManager.CreateAsync(test1, "Test1@123$");
        await userManager.CreateAsync(test2, "Test2@123$");
        await userManager.CreateAsync(test3, "Test3@123$");
        await userManager.CreateAsync(test4, "Test4@123$");

        await userManager.AddToRoleAsync(admin, nameof(UserRoleName.Admin));
        await userManager.AddToRoleAsync(test1, nameof(UserRoleName.User));
        await userManager.AddToRoleAsync(test2, nameof(UserRoleName.User));
        await userManager.AddToRoleAsync(test3, nameof(UserRoleName.User));
        await userManager.AddToRoleAsync(test4, nameof(UserRoleName.User));
    }
}