using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class FilmManagerIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    #region AddFilm

    [Fact]
    public void AddFilm_IsStored_GivenValidData()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var mgr = scope.ServiceProvider.GetService<IManager>();

        string title = "NEW from TEST";

        // Act
        var film = mgr.AddFilm(title, Genre.Drama, new DateTime(2020, 1, 1), 7.5, null, null);

        // Assert
        Assert.NotEqual(Guid.Empty, film.ImdbId);

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

        string invalidTitle = "AB";

        // Act
        var exception = Record.Exception(() =>
            mgr.AddFilm(invalidTitle, Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);

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

        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        var count = ctx.Films.Count(f => f.Title == title);
        Assert.Equal(1, count);
    }

    #endregion

    #region ChangeFilm

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

        // Assert
        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        ctx.ChangeTracker.Clear();
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
        string invalidTitle = "AB";

        film.Title = invalidTitle;

        // Act
        var exception = Record.Exception(() => mgr.ChangeFilm(film));

        // Assert
        Assert.IsType<ValidationException>(exception);

        var ctx = scope.ServiceProvider.GetService<FilmDbContext>();
        ctx.ChangeTracker.Clear();
        var stored = ctx.Films.Single(f => f.ImdbId == film.ImdbId);
        Assert.NotEqual(invalidTitle, stored.Title);
    }

    #endregion
}