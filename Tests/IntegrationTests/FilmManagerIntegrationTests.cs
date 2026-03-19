using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

// Integration tests for Manager.AddFilm and Manager.ChangeFilm.
// These tests run against a real in-memory SQLite database, exercising the full
// BL → DAL → EF Core stack without touching the HTTP layer.
public class FilmManagerIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    [Fact]
    public void AddFilm_WithValidData_ShouldPersistFilmInDatabase()
    {
        // Arrange
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var manager = scope.DbContext.GetService<IManager>()
                      ?? factory.Services.CreateScope().ServiceProvider.GetRequiredService<IManager>();

        // Act
        var film = manager.AddFilm(
            title: "Integration Test Film",
            genre: Genre.Drama,
            releaseDate: new DateTime(2020, 1, 1),
            rating: 7.5,
            director: null,
            maintainer: null);

        // Assert — Guid must be populated and the record must exist in the DB
        Assert.NotEqual(Guid.Empty, film.ImdbId);
        scope.DbContext.ChangeTracker.Clear();
        var persisted = scope.DbContext.Films.Find(film.ImdbId);
        Assert.NotNull(persisted);
        Assert.Equal("Integration Test Film", persisted.Title);
        Assert.Equal(Genre.Drama, persisted.Genre);
    }

    [Fact]
    public void AddFilm_WithDuplicateTitle_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();

        manager.AddFilm("Unique Title Once", Genre.Action, new DateTime(2020, 1, 1), 7.0, null, null);

        // Act & Assert
        Assert.Throws<ValidationException>(() =>
            manager.AddFilm("Unique Title Once", Genre.Comedy, new DateTime(2021, 1, 1), 8.0, null, null));
    }

    [Fact]
    public void AddFilm_WithTitleTooShort_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();

        // Act & Assert — title must be at least 3 characters
        Assert.Throws<ValidationException>(() =>
            manager.AddFilm("AB", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null));
    }

    [Fact]
    public void AddFilm_WithFutureReleaseDate_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();

        // Act & Assert — DateRange(mustBePast: true) rejects future dates
        Assert.Throws<ValidationException>(() =>
            manager.AddFilm("Future Film", Genre.Drama, DateTime.Now.AddYears(1), 7.0, null, null));
    }

    [Fact]
    public void ChangeFilm_WithValidData_ShouldUpdateFilmInDatabase()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();

        var film = manager.AddFilm("Change Test Film", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null);

        // Act
        film.Title = "Change Test Film Updated";
        film.Rating = 9.0;
        manager.ChangeFilm(film);

        // Assert — use the DbContextScope to read back the persisted record
        using var assertScope = factory.CreateDbContextScope<FilmDbContext>();
        var updated = assertScope.DbContext.Films.Find(film.ImdbId);
        Assert.Equal("Change Test Film Updated", updated?.Title);
        Assert.Equal(9.0, updated?.Rating);
    }

    [Fact]
    public void ChangeFilm_WithTitleTooShort_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IManager>();

        var film = manager.AddFilm("Short Change Film", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null);

        // Act & Assert
        film.Title = "AB";
        Assert.Throws<ValidationException>(() => manager.ChangeFilm(film));
    }
}