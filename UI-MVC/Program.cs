using System.Text.Json.Serialization;
using AspNetCoreLiveMonitoring.Extensions;
using FilmManagement.BL;
using FilmManagement.DAL;
using FilmManagement.DAL.EF;
using FilmManagement.UI.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
var connectionString = builder.Configuration.GetConnectionString("FilmDbContextConnection") ?? throw new InvalidOperationException("Connection string 'FilmDbContextConnection' not found.");;

builder.Services.AddDbContext<FilmDbContext>(optionsBuilder => optionsBuilder.UseSqlite(connectionString));
builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddLiveMonitoring();

builder.Services.AddControllersWithViews()
    .AddXmlDataContractSerializerFormatters()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

builder.Services
    .AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FilmDbContext>();

builder.Services.AddAuthorization(options =>
    options.AddPolicy("IsAdmin", policy => policy.RequireRole(nameof(UserRoleName.Admin))));

var app = builder.Build();

//EF code first
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
    var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
    
    if (ctx.CreateDatabase(dropDatabase: true))
    {
        var identitySeeder = new IdentitySeeder(userManager, roleManager);
        identitySeeder.Seed();
        
        DataSeeder.Seed(ctx);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseAndMapLiveMonitoring();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();