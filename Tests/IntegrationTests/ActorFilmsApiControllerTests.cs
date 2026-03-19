using System.Net;
using System.Net.Http.Json;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using FilmManagement.UI.Web.Models.Dto.ActorFilm;
using Microsoft.EntityFrameworkCore;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class ActorFilmsApiControllerTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private (Guid filmId, Guid actorId) GetUnrelatedFilmAndActor(string nonMaintainerEmail)
    {
        using var scope = factory.CreateDbContextScope<FilmDbContext>();

        var film = scope.DbContext.Films
            .Include(f => f.ActorFilms)
            .ThenInclude(af => af.Actor)
            .FirstOrDefault(f =>
                f.Maintainer != null &&
                f.Maintainer.UserName != nonMaintainerEmail);
        Assert.NotNull(film);

        var actorIdsInFilm = film.ActorFilms.Select(af => af.Actor.ImdbId).ToHashSet();
        var actor = scope.DbContext.Actors
            .FirstOrDefault(a => !actorIdsInFilm.Contains(a.ImdbId));
        Assert.NotNull(actor);

        return (film.ImdbId, actor.ImdbId);
    }

    private async Task<(Guid filmId, Guid actorId)> CreateRelationshipAs(
        string email, string password, int? screenTime = null)
    {
        var (filmId, actorId) = GetUnrelatedFilmAndActor(email);
        var client = factory.CreateAuthenticatedClient(email, password);

        var dto = new NewActorFilmDto
        {
            FilmId = filmId,
            ActorId = actorId,
            ScreenTime = screenTime
        };

        var response = await client.PostAsJsonAsync("/api/actorfilms", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return (filmId, actorId);
    }

    #region Create

    [Fact]
    public async Task Create_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var (filmId, actorId) = GetUnrelatedFilmAndActor("test2@test.tld");
        var client = factory.CreateClient();

        var dto = new NewActorFilmDto { FilmId = filmId, ActorId = actorId };

        // Act
        var response = await client.PostAsJsonAsync("/api/actorfilms", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsNonMaintainer_Returns201Created_RelationshipIsStored()
    {
        // Arrange
        var (filmId, actorId) = GetUnrelatedFilmAndActor("test2@test.tld");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        var dto = new NewActorFilmDto { FilmId = filmId, ActorId = actorId, ScreenTime = 45 };

        // Act
        var response = await client.PostAsJsonAsync("/api/actorfilms", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.ActorFilms
            .Include(af => af.Film)
            .Include(af => af.Actor)
            .SingleOrDefault(af =>
                af.Film.ImdbId == filmId &&
                af.Actor.ImdbId == actorId);
        Assert.NotNull(stored);
        Assert.Equal(45, stored.ScreenTime);
    }

    #endregion

    #region UpdateScreenTime

    [Fact]
    public async Task UpdateScreenTime_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var (filmId, actorId) = await CreateRelationshipAs("test1@test.tld", "Test1@123$");
        var client = factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/actorfilms/{filmId}/{actorId}", (int?)60);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateScreenTime_AsNonMaintainer_Returns200Ok_ScreenTimeIsUpdated()
    {
        // Arrange
        var (filmId, actorId) = await CreateRelationshipAs("test1@test.tld", "Test1@123$", screenTime: 30);
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/actorfilms/{filmId}/{actorId}", (int?)99);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        scope.DbContext.ChangeTracker.Clear();
        var updated = scope.DbContext.ActorFilms
            .Include(af => af.Film)
            .Include(af => af.Actor)
            .Single(af =>
                af.Film.ImdbId == filmId &&
                af.Actor.ImdbId == actorId);
        Assert.Equal(99, updated.ScreenTime);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_AsNotLoggedInUser_Returns401Unauthorized()
    {
        // Arrange
        var (filmId, actorId) = await CreateRelationshipAs("test1@test.tld", "Test1@123$");
        var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/actorfilms/{filmId}/{actorId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsNonMaintainer_Returns204NoContent_RelationshipIsRemoved()
    {
        // Arrange
        var (filmId, actorId) = await CreateRelationshipAs("test1@test.tld", "Test1@123$");
        var client = factory.CreateAuthenticatedClient("test2@test.tld", "Test2@123$");

        // Act
        var response = await client.DeleteAsync($"/api/actorfilms/{filmId}/{actorId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var deleted = scope.DbContext.ActorFilms
            .Include(af => af.Film)
            .Include(af => af.Actor)
            .SingleOrDefault(af =>
                af.Film.ImdbId == filmId &&
                af.Actor.ImdbId == actorId);
        Assert.Null(deleted);
    }

    #endregion
}