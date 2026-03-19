using System.Net;
using FilmManagement.UI.MVC.Tests.IntegrationTests.Config;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests;

// Integration tests for FilmController.Create (MVC).
// The Create action is [Authorize(Roles = "Admin")], so three auth scenarios matter:
// anonymous, logged-in non-admin user, and logged-in admin.
//
// Tests exercise the full HTTP pipeline including authentication middleware,
// authorization filters, CSRF protection, antiforgery tokens, and MVC routing.
public class FilmControllerIntegrationTests : IClassFixture<ExtendedWebApplicationFactory<Program>>
{
    private readonly ExtendedWebApplicationFactory<Program> _factory;

    public FilmControllerIntegrationTests(ExtendedWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCreate_AsAnonymous_ShouldRedirectToLogin()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Film/Create");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login",
            response.Headers.Location?.OriginalString ?? "");
    }

    [Fact]
    public async Task GetCreate_AsRegularUser_ShouldReturnForbidden()
    {
        // Arrange — test1 has the "User" role, not "Admin"
        var client = _factory.CreateAuthenticatedClient("test1@test.tld", "Test1@123$");

        // Act
        var response = await client.GetAsync("/Film/Create");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCreate_AsAdmin_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Act
        var response = await client.GetAsync("/Film/Create");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostCreate_AsAdmin_WithValidData_ShouldRedirectToDetails()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("admin@test.tld", "Admin@123$");

        // Fetch the CSRF token from the actual Create page before submitting
        var csrfToken = client.GetCsrfToken("/Film/Create");

        var formData = new Dictionary<string, string>
        {
            { "Title", "POST Integration Test Film" },
            { "SelectedGenres", "4" },      // Drama = 4
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "7.5" },
            { "__RequestVerificationToken", csrfToken }
        };

        // Act
        var response = await client.PostAsync("/Film/Create",
            new FormUrlEncodedContent(formData));

        // Assert — after a successful create the server redirects to Details
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/Film/Details",
            response.RequestMessage?.RequestUri?.PathAndQuery ?? "");
    }

    [Fact]
    public async Task PostCreate_AsAnonymous_ShouldRedirectToLogin()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var formData = new Dictionary<string, string>
        {
            { "Title", "Should Not Be Created" },
            { "SelectedGenres", "4" },
            { "ReleaseDate", "2020-01-01" },
            { "Rating", "7.5" }
        };

        // Act
        var response = await client.PostAsync("/Film/Create",
            new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login",
            response.Headers.Location?.OriginalString ?? "");
    }
}