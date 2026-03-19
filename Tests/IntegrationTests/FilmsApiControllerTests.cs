using System.Net;
using System.Net.Http.Json;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using FilmManagement.UI.Web.Models.Dto.Film;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class FilmsApiControllerTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private Guid GetFilmMaintainedBy(string maintainerEmail)
    {
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var film = scope.DbContext.Films
            .FirstOrDefault(f => f.Maintainer != null && f.Maintainer.UserName == maintainerEmail);
        Assert.NotNull(film);
        return film.ImdbId;
    }

    #region UpdateRating

    [Fact]
    public async Task UpdateRating_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync($"/api/films/{filmId}/rating", 8.0);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_AsNonMaintainerNonAdmin_Returns403Forbidden()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = await client.PutAsJsonAsync($"/api/films/{filmId}/rating", 8.0);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_AsMaintainer_Returns200Ok_RatingIsUpdated()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");
        double newRating = 9.5;

        // Act
        var response = await client.PutAsJsonAsync($"/api/films/{filmId}/rating", newRating);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        scope.DbContext.ChangeTracker.Clear();
        var updated = scope.DbContext.Films.Single(f => f.ImdbId == filmId);
        Assert.Equal(newRating, updated.Rating);
    }

    [Fact]
    public async Task UpdateRating_AsAdmin_Returns200Ok_EvenWhenNotMaintainer()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = await client.PutAsJsonAsync($"/api/films/{filmId}/rating", 6.0);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_Returns400BadRequest_GivenRatingAbove10()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = await client.PutAsJsonAsync($"/api/films/{filmId}/rating", 11.0);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_Returns404NotFound_GivenNonExistentFilmId()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = await client.PutAsJsonAsync($"/api/films/{Guid.NewGuid()}/rating", 7.0);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/films/{filmId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsNonMaintainerNonAdmin_Returns403Forbidden_FilmNotDeleted()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = await client.DeleteAsync($"/api/films/{filmId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var film = scope.DbContext.Films.SingleOrDefault(f => f.ImdbId == filmId);
        Assert.NotNull(film);
    }

    [Fact]
    public async Task Delete_AsMaintainer_Returns204NoContent_FilmIsRemoved()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = await client.DeleteAsync($"/api/films/{filmId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var deleted = scope.DbContext.Films.SingleOrDefault(f => f.ImdbId == filmId);
        Assert.Null(deleted);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_AsAdmin_StoresFilm_Returns201Created_GivenValidData()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");
        string title = "API Created from TEST";

        var dto = new NewFilmDto
        {
            Title = title,
            Genre = BL.Domain.Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.5
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/films", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Matches(@"(?i)^.*/api/films/", response.Headers.Location?.OriginalString);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var created = scope.DbContext.Films.SingleOrDefault(f => f.Title == title);
        Assert.NotNull(created);
        Assert.Equal(title, created.Title);
    }

    [Fact]
    public async Task Create_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var client = factory.CreateClient();
        var dto = new NewFilmDto
        {
            Title = "Should Not Be Created",
            Genre = BL.Domain.Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.0
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/films", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}