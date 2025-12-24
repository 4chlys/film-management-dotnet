using System.Diagnostics;
using FilmManagement.BL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmManagement.DAL.EF;

public class FilmDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Film> Films { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<ActorFilm> ActorFilms { get; set; }
    public DbSet<Director> Directors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var contextName = GetType().Name;
            optionsBuilder.UseSqlite($"Data Source=_ERROR_{contextName}_Not_Configured_.db");
        }

        optionsBuilder.LogTo(message => Debug.WriteLine(message), LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Actor>()
            .Ignore(a => a.Age);

        modelBuilder.Entity<Director>()
            .HasIndex(d => d.Name);
        
        modelBuilder.Entity<Film>()
            .HasIndex(f => f.Title)
            .IsUnique();
        
        modelBuilder.Entity<Film>()
            .Property<Guid?>("DirectorFK_Shadow");
        
        modelBuilder.Entity<ActorFilm>()
            .Property<Guid>("ActorFK_Shadow");
        modelBuilder.Entity<ActorFilm>()
            .Property<Guid>("FilmFK_Shadow");
        modelBuilder.Entity<ActorFilm>()
            .HasKey("ActorFK_Shadow", "FilmFK_Shadow");

        modelBuilder.Entity<Film>()
            .HasOne(film => film.Director)
            .WithMany(director => director.Films)
            .HasForeignKey("DirectorFK_Shadow")
            .IsRequired(false);

        modelBuilder.Entity<ActorFilm>()
            .HasOne(af => af.Actor)
            .WithMany(actor => actor.ActorFilms)
            .HasForeignKey("ActorFK_Shadow")
            .IsRequired();

        modelBuilder.Entity<ActorFilm>()
            .HasOne(af => af.Film)
            .WithMany(film => film.ActorFilms)
            .HasForeignKey("FilmFK_Shadow")
            .IsRequired();
        
        base.OnModelCreating(modelBuilder);
    }

    public bool CreateDatabase(bool dropDatabase)
    {
        if (dropDatabase)
        {
            Database.EnsureDeleted();
        }
        return Database.EnsureCreated();
    }
}