using FilmManagement.BL;
using FilmManagement.DAL;
using FilmManagement.DAL.EF;
using FilmManagement.UI.CA;
using Microsoft.EntityFrameworkCore;

string dbPath = "filmmanagementdb.db";

string connectionString = $"Data source={dbPath}";

DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder<FilmDbContext>();
optionsBuilder.UseSqlite(connectionString);
FilmDbContext context = new FilmDbContext(optionsBuilder.Options);

if (context.CreateDatabase(dropDatabase: true))
{
    DataSeeder.Seed(context);
}

IRepository filmRepository = new EfRepository(context);
IManager filmManager = new Manager(filmRepository);
ConsoleUi consoleUi = new ConsoleUi(filmManager);

consoleUi.Run();

