using System.Text.Json.Serialization;
using AspNetCoreLiveMonitoring.Extensions;
using FilmManagement.BL;
using FilmManagement.DAL;
using FilmManagement.DAL.EF;
using FilmManagement.UI.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

var connectionString = builder.Configuration.GetConnectionString("FilmDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'FilmDbContextConnection' not found.");

builder.Services.AddDbContext<FilmDbContext>(optionsBuilder =>
    optionsBuilder.UseSqlite(connectionString));

builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddLiveMonitoring();

builder.Services.AddControllersWithViews()
    .AddXmlDataContractSerializerFormatters()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<FilmDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    var defaultOnRedirectToLogin = options.Events.OnRedirectToLogin;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        return defaultOnRedirectToLogin(context);
    };

    var defaultOnRedirectToAccessDenied = options.Events.OnRedirectToAccessDenied;
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        return defaultOnRedirectToAccessDenied(context);
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<FilmDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (ctx.CreateDatabase(dropDatabase: true))
    {
        var identitySeeder = new IdentitySeeder(userManager, roleManager);
        await identitySeeder.SeedAsync();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();