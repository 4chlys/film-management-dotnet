using System.Net;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class FilmControllerIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    [Fact]
    public void Index_ShowsAllFilms_AsAnonymousUser()
    {
        // Arrange
        var client = factory.CreateClient();
        var url = "/Film/Index";

        // Act
        var response = client.GetAsync(url).Result;
        var body = response.Content.ReadAsStringAsync().Result;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<title>Films", body);
    }

    [Fact]
    public void Create_Post_AsNotLoggedInUser_ReturnsRedirectToLogin()
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var url = "/Film/Create";

        // Act
        var response = client.PostAsync(url, null).Result;

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Identity/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public void Create_Post_AsRegularUser_ReturnsRedirectToAccessDenied_FilmNotStored()
    {
        // Arrange — test1 has "User" role, not "Admin"
        var client = factory.CreateAuthenticatedClient(
            "test1@test.tld", "Test1@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var url = "/Film/Create";
        string title = "Should Not Be Created";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Title", title },
            { "SelectedGenres", "4" },
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "7.5" }
        });

        // Act
        var response = client.PostAsync(url, content).Result;

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Identity/Account/AccessDenied", response.Headers.Location?.AbsolutePath);

        // DB check — nothing was stored
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.Films.SingleOrDefault(f => f.Title == title);
        Assert.Null(stored);
    }

    [Fact]
    public void Create_AsAdmin_ReturnsBadRequest_GivenInvalidCsrf()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "admin@test.tld", "Admin@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var url = "/Film/Create";

        var data = new Dictionary<string, string>
        {
            { "Title", "Some Film" },
            { "SelectedGenres", "4" },
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "7.5" },
            { "__RequestVerificationToken", "INVALID_CSRF_TOKEN" }
        };

        // Act
        var response = client.PostAsync(url, new FormUrlEncodedContent(data)).Result;

        // Assert — [AutoValidateAntiforgeryToken] rejects an invalid token with 400
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void Create_Post_AsAdmin_FilmNotStored_ShowsFormWithErrors_GivenInvalidData_WithValidCsrf()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "admin@test.tld", "Admin@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var url = "/Film/Create";

        var csrfToken = client.GetCsrfToken(url);

        var data = new Dictionary<string, string>
        {
            { "Title", "" },              // invalid: [Required] + MinimumLength = 3
            { "SelectedGenres", "4" },
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "7.5" },
            { "__RequestVerificationToken", csrfToken }
        };

        // Act
        var response = client.PostAsync(url, new FormUrlEncodedContent(data)).Result;
        var body = response.Content.ReadAsStringAsync().Result;

        // Assert — form is re-shown with a validation error
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field is required", body);

        // DB check — nothing stored
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.Films.SingleOrDefault(f => f.Title == "");
        Assert.Null(stored);
    }

    [Fact]
    public void Create_Post_AsAdmin_StoresFilm_RedirectsToDetails_GivenValidData_WithValidCsrf()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "admin@test.tld", "Admin@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var url = "/Film/Create";

        var csrfToken = client.GetCsrfToken(url);

        string title = "NEW from TEST";
        var data = new Dictionary<string, string>
        {
            { "Title", title },
            { "SelectedGenres", "4" },   // Drama = 4
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "7.5" },
            { "__RequestVerificationToken", csrfToken }
        };

        // Act
        var response = client.PostAsync(url, new FormUrlEncodedContent(data)).Result;

        // Assert — successful create redirects to Details
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Matches(@"^.*/Film/Details", response.Headers.Location?.OriginalString);

        // DB check — film was persisted
        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.Films.SingleOrDefault(f => f.Title == title);
        Assert.NotNull(stored);
    }
}