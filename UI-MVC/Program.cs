using System.Text.Json.Serialization;
using AspNetCoreLiveMonitoring.Extensions;
using FilmManagement.BL;
using FilmManagement.DAL;
using FilmManagement.DAL.EF;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FilmDbContext>(optionsBuilder => optionsBuilder.UseSqlite("Data Source=filmmanagementdb.sqlite"));
builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<IManager, Manager>();
builder.Services.AddLiveMonitoring();

builder.Services.AddControllersWithViews()
    .AddXmlDataContractSerializerFormatters()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

var app = builder.Build();

//EF code first
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
    if (ctx.CreateDatabase(dropDatabase: true))
    {
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