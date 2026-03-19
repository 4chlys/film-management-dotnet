using System.Net;
using System.Text;
using System.Text.Json;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

// Integration tests for PUT /api/films/{id}/rating.
//
// This endpoint implements the "complex authorization" requirement:
//   - Anonymous              → 401 (API must never redirect to login page)
//   - Not maintainer, not admin → 403
//   - Maintainer of this film → 200
//   - Admin (any film)       → 200 (admin overrides ownership)
//
// Cite this class in the README under "complex authorization":
// FilmManagement.UI.MVC.Tests.IntegrationTests.FilmsApiAuthorizationTests
public class FilmsApiControllerTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private static StringContent JsonBody(double value) =>
        new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

    // Looks up a film in the DB whose Maintainer matches the given email so tests
    // know exactly which film they should (or should not) be able to edit.
    private Guid GetFilmMaintainedBy(string maintainerEmail)
    {
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var film = scope.DbContext.Films
            .FirstOrDefault(f => f.Maintainer != null
                                 && f.Maintainer.UserName == maintainerEmail);
        Assert.NotNull(film);
        return film.ImdbId;
    }

    [Fact]
    public async Task UpdateRating_AsAnonymous_ShouldReturn401()
    {
        // Arrange — use a plain (unauthenticated) client
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateClient();

        // Act
        var response = await client.PutAsync($"/api/films/{filmId}/rating", JsonBody(8.0));

        // Assert — the API must return 401, not a redirect to the login page
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_AsNonMaintainerNonAdmin_ShouldReturn403()
    {
        // Arrange — film belongs to test1, we log in as test2
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = await client.PutAsync($"/api/films/{filmId}/rating", JsonBody(8.0));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_AsMaintainer_ShouldReturn200()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = await client.PutAsync($"/api/films/{filmId}/rating", JsonBody(9.5));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_AsAdmin_ShouldReturn200_EvenWhenNotMaintainer()
    {
        // Arrange — admin@test.tld is NOT the maintainer of test1's film
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = await client.PutAsync($"/api/films/{filmId}/rating", JsonBody(6.0));

        // Assert — admin role overrides the ownership check
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_WithRatingAbove10_ShouldReturn400()
    {
        // Arrange
        var filmId = GetFilmMaintainedBy("test1@test.tld");
        var client = factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = await client.PutAsync($"/api/films/{filmId}/rating", JsonBody(11.0));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRating_ForNonExistentFilm_ShouldReturn404()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = await client.PutAsync($"/api/films/{Guid.NewGuid()}/rating", JsonBody(7.0));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}