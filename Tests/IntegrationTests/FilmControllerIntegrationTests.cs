using System.Net;
using FilmManagement.DAL.EF;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

public class FilmControllerIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    #region Index

    [Fact]
    public async Task Index_ShowsAllFilms_AsAnonymousUser()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Film/Index");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<title>Films", body);
    }

    #endregion

    #region Create POST

    [Fact]
    public async Task Create_Post_AsNotLoggedInUser_ReturnsRedirectToLogin()
    {
        // Arrange
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.PostAsync("/Film/Create", null);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Identity/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Create_Post_AsRegularUser_ReturnsRedirectToAccessDenied_FilmNotStored()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "test1@test.tld", "Test1@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        string title = "Should Not Be Created";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Title", title },
            { "Genre", "4" },
            { "ReleaseDate", "01/01/2020" },
            { "Rating", "8" }
        });

        // Act
        var response = await client.PostAsync("/Film/Create", content);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Identity/Account/AccessDenied", response.Headers.Location?.AbsolutePath);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.Films.SingleOrDefault(f => f.Title == title);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Create_AsAdmin_ReturnsBadRequest_GivenInvalidCsrf()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "admin@test.tld", "Admin@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var data = new Dictionary<string, string>
        {
            { "Title", "Some Film" },
            { "Genre", "4" },
            { "ReleaseDate", "01/01/2020" },
            { "Rating", "8" },
            { "__RequestVerificationToken", "INVALID_CSRF_TOKEN" }
        };

        // Act
        var response = await client.PostAsync("/Film/Create", new FormUrlEncodedContent(data));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Post_AsAdmin_FilmNotStored_ShowsFormWithErrors_GivenInvalidData_WithValidCsrf()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "admin@test.tld", "Admin@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var csrfToken = client.GetCsrfToken("/Film/Create");

        var data = new Dictionary<string, string>
        {
            { "Title", "" },
            { "Genre", "4" },
            { "ReleaseDate", "01/01/2020" },
            { "Rating", "8" },
            { "__RequestVerificationToken", csrfToken }
        };

        // Act
        var response = await client.PostAsync("/Film/Create", new FormUrlEncodedContent(data));
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field is required", body);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.Films.SingleOrDefault(f => f.Title == "");
        Assert.Null(stored);
    }

    [Fact]
    public async Task Create_Post_AsAdmin_StoresFilm_RedirectsToDetails_GivenValidData_WithValidCsrf()
    {
        // Arrange
        var client = factory.CreateAuthenticatedClient(
            "admin@test.tld", "Admin@123$",
            options: new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var csrfToken = client.GetCsrfToken("/Film/Create");

        string title = "NEW from TEST";
        var data = new Dictionary<string, string>
        {
            { "Title", title },
            { "Genre", "4" },
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "8" },
            { "__RequestVerificationToken", csrfToken }
        };

        // Act
        var response = await client.PostAsync("/Film/Create", new FormUrlEncodedContent(data));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Matches(@"^.*/Film/Details", response.Headers.Location?.OriginalString);

        using var scope = factory.CreateDbContextScope<FilmDbContext>();
        var stored = scope.DbContext.Films.SingleOrDefault(f => f.Title == title);
        Assert.NotNull(stored);
    }

    #endregion
}