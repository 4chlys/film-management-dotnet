using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using FilmManagement.UI.Web.Models.Dto.Film;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class FilmsApiControllerTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private static StringContent JsonBody(double value) =>
        new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

    private Guid GetFilmMaintainedBy(string maintainerEmail)
    {
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var film = scope.DbContext.Films
            .FirstOrDefault(f => f.Maintainer != null && f.Maintainer.UserName == maintainerEmail);
        Assert.NotNull(film);
        return film.ImdbId;
    }

    [Fact]
    public void UpdateRating_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateClient();

        // Act
        var response = client.PutAsync($"/api/films/{filmId}/rating", JsonBody(8.0)).Result;

        // Assert — API must not redirect to login; must return 401 directly
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void UpdateRating_AsNonMaintainerNonAdmin_Returns403Forbidden()
    {
        // Arrange — film belongs to test1, we log in as test2
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = client.PutAsync($"/api/films/{filmId}/rating", JsonBody(8.0)).Result;

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public void UpdateRating_AsMaintainer_Returns200Ok_RatingIsUpdated()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");
        double newRating = 9.5;

        // Act
        var response = client.PutAsync($"/api/films/{filmId}/rating", JsonBody(newRating)).Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // DB check — rating was persisted
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        scope.DbContext.ChangeTracker.Clear();
        var updated = scope.DbContext.Films.Single(f => f.ImdbId == filmId);
        Assert.Equal(newRating, updated.Rating);
    }

    [Fact]
    public void UpdateRating_AsAdmin_Returns200Ok_EvenWhenNotMaintainer()
    {
        // Arrange — admin is NOT the maintainer of test1's film
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = client.PutAsync($"/api/films/{filmId}/rating", JsonBody(6.0)).Result;

        // Assert — admin role overrides the ownership check
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void UpdateRating_Returns400BadRequest_GivenRatingAbove10()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = client.PutAsync($"/api/films/{filmId}/rating", JsonBody(11.0)).Result;

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void UpdateRating_Returns404NotFound_GivenNonExistentFilmId()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = client.PutAsync($"/api/films/{Guid.NewGuid()}/rating", JsonBody(7.0)).Result;

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void Delete_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateClient();

        // Act
        var response = client.DeleteAsync($"/api/films/{filmId}").Result;

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void Delete_AsNonMaintainerNonAdmin_Returns403Forbidden_FilmNotDeleted()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = client.DeleteAsync($"/api/films/{filmId}").Result;

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // DB check — film must still exist
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var film = scope.DbContext.Films.SingleOrDefault(f => f.ImdbId == filmId);
        Assert.NotNull(film);
    }

    [Fact]
    public void Delete_AsMaintainer_Returns204NoContent_FilmIsRemoved()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = client.DeleteAsync($"/api/films/{filmId}").Result;

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // DB check — film must be gone
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var deleted = scope.DbContext.Films.SingleOrDefault(f => f.ImdbId == filmId);
        Assert.Null(deleted);
    }

    [Fact]
    public void Create_AsAdmin_StoresFilm_Returns201Created_GivenValidData()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");
        var url = "/api/films";
        string title = "API Created from TEST";

        var dto = new NewFilmDto
        {
            Title = title,
            Genre = BL.Domain.Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.5
        };

        // Act
        var response = client.PostAsJsonAsync(url, dto).Result;

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Matches(@"^.*/api/films/", response.Headers.Location?.OriginalString);

        // DB check
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var created = scope.DbContext.Films.SingleOrDefault(f => f.Title == title);
        Assert.NotNull(created);
        Assert.Equal(title, created.Title);
    }

    [Fact]
    public void Create_AsNotLoggedInUser_Returns401Unauthorized()
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
        var response = client.PostAsJsonAsync("/api/films", dto).Result;

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}