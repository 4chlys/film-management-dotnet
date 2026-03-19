using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using FilmManagement.DAL.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;



namespace FilmManagement.UI.MVC.Tests.IntegrationTests.Config;

public class ExtendedWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    // Use of in-memory sqlite database
    public SqliteConnection SqliteInMemoryConnection { get; private set; }

    public IDbContextScope<TContext> CreateDbContextScope<TContext>()
        where TContext : DbContext
    {
        var scope = Services.CreateScope();
        return new DbContextScope<TContext>(scope);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use of in-memory sqlite database
        builder.ConfigureServices(services =>
        {
            var dbContextOptionsDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<FilmDbContext>));
            services.Remove(dbContextOptionsDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbConnection));
            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
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

    public HttpClient CreateAuthenticatedClient(string email, string password, string username = null,
        WebApplicationFactoryClientOptions options = null)
    {
        var loginClient = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
            // HandleCookies = true  // this is the default
        });

        var loginUrl = "/Identity/Account/Login";
        // get login-page to capture CSRF-token
        var csrfToken = loginClient.GetCsrfToken(loginUrl);
        // submit login-form with credentials
        var loginPostResponse = loginClient.PostAsync(loginUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "UserName", username ?? email },
            { "Email", email },
            { "Password", password },
            { "__RequestVerificationToken", csrfToken }
        })).Result;
        var content = loginPostResponse.Content.ReadAsStringAsync().Result;

        var cookieContainer = new CookieContainer();
        foreach (var cookieHeader in loginPostResponse.Headers.GetValues("Set-Cookie"))
        {
            cookieContainer.SetCookies(loginClient.BaseAddress!, cookieHeader);
        }

        var opts = options ?? ClientOptions;
        var client = CreateDefaultClient(opts.BaseAddress, opts.CreateHandlers(cookieContainer));

        return client;
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
    public TContext DbContext { get; }

    public DbContextScope(IServiceScope scope)
    {
        _scope = scope;
        DbContext = _scope.ServiceProvider.GetRequiredService<TContext>();
    }

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

        var loginGetResponse = client.GetAsync(rootRelativeUrl).Result;
        loginGetResponse.EnsureSuccessStatusCode();

        var html = loginGetResponse.Content.ReadAsStringAsync().Result;
        var tokenMatch = Regex.Match(html, @"name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");

        if (!tokenMatch.Success || tokenMatch.Groups.Count < 2)
            throw new InvalidOperationException(
                "Could not find '__RequestVerificationToken' hidden field in the page.");

        var csrfToken = tokenMatch.Groups[1].Value;

        return csrfToken;
    }
}

internal static class WebApplicationFactoryClientOptionsExtensions
{
    internal static DelegatingHandler[] CreateHandlers(this WebApplicationFactoryClientOptions options,
        CookieContainer container = null)
    {
        return CreateHandlersCore().ToArray();

        IEnumerable<DelegatingHandler> CreateHandlersCore()
        {
            if (options.AllowAutoRedirect)
            {
                yield return new RedirectHandler(options.MaxAutomaticRedirections);
            }

            if (options.HandleCookies)
            {
                if (container == null)
                {
                    yield return new CookieContainerHandler();
                }
                else
                {
                    yield return new CookieContainerHandler(container);
                }
            }
        }
    }
}
