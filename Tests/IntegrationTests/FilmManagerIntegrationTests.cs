using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class FilmManagerIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    [Fact]
    public void AddFilm_IsStored_GivenValidData()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        string title = "NEW from TEST";

        // Act
        var film = mgr.AddFilm(title, Genre.Drama, new DateTime(2020, 1, 1), 7.5, null, null);

        // Assert — object returned with a real ID
        Assert.NotEqual(Guid.Empty, film.ImdbId);

        // DB check — query via the context in the same scope
        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        var stored = ctx.Films.SingleOrDefault(f => f.ImdbId == film.ImdbId);
        Assert.NotNull(stored);
        Assert.Equal(title, stored.Title);
        Assert.Equal(Genre.Drama, stored.Genre);
    }

    [Fact]
    public void AddFilm_NotStored_ThrowsValidationException_GivenTitleTooShort()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        string invalidTitle = "AB"; // minimum is 3 characters

        // Act
        var exception = Record.Exception(() =>
            mgr.AddFilm(invalidTitle, Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);

        // DB check — nothing should have been written
        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        var stored = ctx.Films.SingleOrDefault(f => f.Title == invalidTitle);
        Assert.Null(stored);
    }

    [Fact]
    public void AddFilm_NotStored_ThrowsValidationException_GivenFutureReleaseDate()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        // Act
        var exception = Record.Exception(() =>
            mgr.AddFilm("Future Film", Genre.Drama, DateTime.Now.AddYears(1), 7.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);
    }

    [Fact]
    public void AddFilm_NotStored_ThrowsValidationException_GivenDuplicateTitle()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        string title = "Duplicate Title";
        mgr.AddFilm(title, Genre.Action, new DateTime(2020, 1, 1), 7.0, null, null);

        // Act
        var exception = Record.Exception(() =>
            mgr.AddFilm(title, Genre.Comedy, new DateTime(2021, 1, 1), 8.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);

        // DB check — only one record with this title should exist
        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        var count = ctx.Films.Count(f => f.Title == title);
        Assert.Equal(1, count);
    }

    [Fact]
    public void ChangeFilm_IsStored_GivenValidFilm()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        var film = mgr.AddFilm("Original Title", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null);
        film.Title = "UPDATED from TEST";
        film.Rating = 9.0;

        // Act
        mgr.ChangeFilm(film);

        // Assert — DB check in same scope
        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        ctx.ChangeTracker.Clear(); // clear the local cache so we read from DB
        var updated = ctx.Films.Single(f => f.ImdbId == film.ImdbId);
        Assert.Equal("UPDATED from TEST", updated.Title);
        Assert.Equal(9.0, updated.Rating);
    }

    [Fact]
    public void ChangeFilm_NotStored_ThrowsValidationException_GivenInvalidTitle()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        var film = mgr.AddFilm("Valid Title", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null);
        string invalidTitle = "AB"; // too short

        film.Title = invalidTitle;

        // Act
        var exception = Record.Exception(() => mgr.ChangeFilm(film));

        // Assert
        Assert.IsType<ValidationException>(exception);

        // DB check — title must not have changed
        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        ctx.ChangeTracker.Clear();
        var stored = ctx.Films.Single(f => f.ImdbId == film.ImdbId);
        Assert.NotEqual(invalidTitle, stored.Title);
    }
}