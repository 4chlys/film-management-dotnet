using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using FilmManagement.DAL.EF;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FilmManagement.UI.MVC.Tests.IntegrationTests.Config;

public class ExtendedWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    // Exposed so tests can open additional connections to the same DB if needed.
    public SqliteConnection SqliteInMemoryConnection { get; private set; }
 
    public IDbContextScope<TContext> CreateDbContextScope<TContext>()
        where TContext : DbContext
    {
        var scope = Services.CreateScope();
        return new DbContextScope<TContext>(scope);
    }
 
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext and any existing DbConnection registrations.
            var dbContextOptionsDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<FilmDbContext>));
            services.Remove(dbContextOptionsDescriptor);
 
            var dbConnectionDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbConnection));
            services.Remove(dbConnectionDescriptor);
 
            // Register a single open SqliteConnection as a singleton.
            // EF must not close this connection — closing an in-memory SQLite
            // connection destroys the database.
            services.AddSingleton<DbConnection>(container =>
            {
                SqliteInMemoryConnection = new SqliteConnection("DataSource=:memory:");
                var connection = SqliteInMemoryConnection;
                connection.Open();
                return connection;
            });
 
            services.AddDbContext<FilmDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
 
        builder.UseEnvironment("Development");
    }
 
    // Performs a real cookie-based login and returns an HttpClient that carries
    // the resulting authentication cookie on every subsequent request.
    // Pass AllowAutoRedirect = false via options when you need to capture 302s.
    public HttpClient CreateAuthenticatedClient(string email, string password,
        string username = null, WebApplicationFactoryClientOptions options = null)
    {
        var loginClient = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
 
        var loginUrl = "/Identity/Account/Login";
 
        // Fetch the antiforgery token from the login page.
        var csrfToken = loginClient.GetCsrfToken(loginUrl);
 
        // POST credentials. ASP.NET Core Identity expects Input.Email / Input.Password.
        var loginPostResponse = loginClient.PostAsync(loginUrl, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "UserName", username ?? email },
                { "Input.Email", email },
                { "Input.Password", password },
                { "__RequestVerificationToken", csrfToken }
            })).Result;
 
        // Harvest all Set-Cookie headers from the 302 response.
        var cookieContainer = new CookieContainer();
        if (loginPostResponse.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
        {
            foreach (var cookieHeader in cookieHeaders)
                cookieContainer.SetCookies(loginClient.BaseAddress!, cookieHeader);
        }
 
        var opts = options ?? ClientOptions;
        return CreateDefaultClient(opts.BaseAddress, opts.CreateHandlers(cookieContainer));
    }
}

public interface IDbContextScope<out TContext> : IDisposable
    where TContext : DbContext
{
    TContext DbContext { get; }
}
 
public class DbContextScope<TContext> : IDbContextScope<TContext>
    where TContext : DbContext
{
    private readonly IServiceScope _scope;
 
    public DbContextScope(IServiceScope scope)
    {
        _scope = scope;
        DbContext = _scope.ServiceProvider.GetRequiredService<TContext>();
    }
 
    public TContext DbContext { get; }
 
    public void Dispose() => _scope.Dispose();
}
 
public static class HttpClientExtensions
{
    public static string GetCsrfToken(this HttpClient client, string rootRelativeUrl)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrWhiteSpace(rootRelativeUrl))
            throw new ArgumentException("Url cannot be null or empty.", nameof(rootRelativeUrl));
        if (!rootRelativeUrl.StartsWith("/"))
            throw new ArgumentException("Url isn't root relative.", nameof(rootRelativeUrl));
 
        var response = client.GetAsync(rootRelativeUrl).Result;
        response.EnsureSuccessStatusCode();
 
        var html = response.Content.ReadAsStringAsync().Result;
 
        var tokenMatch = Regex.Match(
            html,
            @"name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
 
        if (!tokenMatch.Success || tokenMatch.Groups.Count < 2)
            throw new InvalidOperationException(
                "Could not find '__RequestVerificationToken' hidden field in the page.");
 
        return tokenMatch.Groups[1].Value;
    }
}
 
internal static class WebApplicationFactoryClientOptionsExtensions
{
    internal static DelegatingHandler[] CreateHandlers(
        this WebApplicationFactoryClientOptions options,
        CookieContainer container = null)
    {
        return CreateHandlersCore().ToArray();
 
        IEnumerable<DelegatingHandler> CreateHandlersCore()
        {
            if (options.AllowAutoRedirect)
                yield return new RedirectHandler(options.MaxAutomaticRedirections);
 
            if (options.HandleCookies)
                yield return container is null
                    ? new CookieContainerHandler()
                    : new CookieContainerHandler(container);
        }
    }
}