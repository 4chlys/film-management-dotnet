using System.Security.Claims;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.UI.Web.Controllers;
using FilmManagement.UI.Web.Models.Film;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FilmManagement.UI.MVC.Tests.UnitTests;

public class FilmControllerUnitTests
{
    private readonly Mock<IManager> _mockManager = new();
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly FilmController _ctr;

    public FilmControllerUnitTests()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _ctr = new FilmController(_mockManager.Object, _mockUserManager.Object);
    }

    private void SetUser(string username, bool isAdmin = false)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username) };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        _ctr.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mockAuthType"))
            }
        };
    }

    #region Create POST

    [Fact]
    public async Task Create_Post_ReturnsCreateView_DoesNotCallManager_GivenInvalidModelState()
    {
        // Arrange
        SetUser("admin@test.tld", isAdmin: true);

        var vm = new NewFilmViewModel
        {
            Title = "",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.5,
            SelectedActorIds = new List<Guid>()
        };

        _ctr.ModelState.AddModelError("Title", "The Title field is required");

        _mockManager.Setup(mgr => mgr.AddFilmWithActors(
                It.IsAny<string>(), It.IsAny<Genre>(), It.IsAny<DateTime>(),
                It.IsAny<double>(), It.IsAny<Guid?>(), It.IsAny<List<Guid>>(),
                It.IsAny<IdentityUser>()))
            .Verifiable(Times.Never);

        _mockManager.Setup(mgr => mgr.GetAllDirectors()).Returns(new List<Director>());
        _mockManager.Setup(mgr => mgr.GetAllActors()).Returns(new List<Actor>());

        // Act
        var result = await _ctr.Create(vm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Create", viewResult.ViewName ?? nameof(FilmController.Create));

        _mockManager.VerifyAll();
    }

    [Fact]
    public async Task Create_Post_ReturnsRedirectToDetails_CallsManagerOnce_GivenValidData()
    {
        // Arrange
        SetUser("admin@test.tld", isAdmin: true);

        var identityUser = new IdentityUser { UserName = "admin@test.tld" };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(identityUser);

        var vm = new NewFilmViewModel
        {
            Title = "New Film",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.5,
            DirectorId = null,
            SelectedActorIds = new List<Guid>()
        };

        var createdFilm = new Film
        {
            ImdbId = Guid.NewGuid(),
            Title = vm.Title,
            Genre = vm.Genre,
            ReleaseDate = vm.ReleaseDate,
            Rating = vm.Rating
        };

        _mockManager.Setup(mgr => mgr.AddFilmWithActors(
                vm.Title, vm.Genre, vm.ReleaseDate, vm.Rating,
                vm.DirectorId, vm.SelectedActorIds, identityUser))
            .Returns(createdFilm)
            .Verifiable(Times.Exactly(1));

        // Act
        var result = await _ctr.Create(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(FilmController.Details), redirect.ActionName);
        Assert.Equal(createdFilm.ImdbId, redirect.RouteValues!["id"]);

        _mockManager.VerifyAll();
    }

    #endregion

    #region Edit POST

    [Fact]
    public async Task Edit_Post_ReturnsEditView_DoesNotCallManager_GivenInvalidModelState()
    {
        // Arrange
        var filmId = Guid.NewGuid();
        SetUser("admin@test.tld", isAdmin: true);

        var vm = new EditFilmViewModel
        {
            ImdbId = filmId,
            Title = "",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.5,
            SelectedActorIds = new List<Guid>()
        };

        _ctr.ModelState.AddModelError("Title", "The Title field is required");

        _mockManager.Setup(mgr => mgr.ChangeFilmWithActors(
                It.IsAny<Film>(), It.IsAny<Guid?>(), It.IsAny<List<Guid>>()))
            .Verifiable(Times.Never);

        _mockManager.Setup(mgr => mgr.GetAllDirectors()).Returns(new List<Director>());
        _mockManager.Setup(mgr => mgr.GetAllActors()).Returns(new List<Actor>());

        // Act
        var result = await _ctr.Edit(filmId, vm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Edit", viewResult.ViewName ?? nameof(FilmController.Edit));

        _mockManager.VerifyAll();
    }

    [Fact]
    public async Task Edit_Post_ReturnsRedirectToDetails_CallsManagerOnce_GivenValidData_AsMaintainer()
    {
        // Arrange
        var filmId = Guid.NewGuid();
        var maintainer = new IdentityUser { Id = "maintainer-id", UserName = "test1@test.tld" };

        SetUser(maintainer.UserName, isAdmin: false);

        var existingFilm = new Film
        {
            ImdbId = filmId,
            Title = "Original Title",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.0,
            Maintainer = maintainer
        };

        var vm = new EditFilmViewModel
        {
            ImdbId = filmId,
            Title = "Updated Title",
            Genre = Genre.Action,
            ReleaseDate = new DateTime(2021, 1, 1),
            Rating = 8.0,
            SelectedActorIds = new List<Guid>()
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(maintainer);

        _mockManager.Setup(mgr => mgr.GetFilmWithMaintainer(filmId))
            .Returns(existingFilm);

        _mockManager.Setup(mgr => mgr.ChangeFilmWithActors(
                It.IsAny<Film>(), It.IsAny<Guid?>(), It.IsAny<List<Guid>>()))
            .Verifiable(Times.Exactly(1));

        // Act
        var result = await _ctr.Edit(filmId, vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(FilmController.Details), redirect.ActionName);

        _mockManager.VerifyAll();
    }

    [Fact]
    public async Task Edit_Post_ReturnsAccessDenied_DoesNotCallManager_GivenUserIsNotMaintainerOrAdmin()
    {
        // Arrange
        var filmId = Guid.NewGuid();
        var maintainer = new IdentityUser { Id = "maintainer-id", UserName = "test1@test.tld" };
        var otherUser = new IdentityUser { Id = "other-id", UserName = "test2@test.tld" };

        SetUser(otherUser.UserName, isAdmin: false);

        var existingFilm = new Film
        {
            ImdbId = filmId,
            Title = "Some Film",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.0,
            Maintainer = maintainer
        };

        var vm = new EditFilmViewModel
        {
            ImdbId = filmId,
            Title = "Attempted Edit",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.0,
            SelectedActorIds = new List<Guid>()
        };

        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(otherUser);

        _mockManager.Setup(mgr => mgr.GetFilmWithMaintainer(filmId))
            .Returns(existingFilm);

        _mockManager.Setup(mgr => mgr.ChangeFilmWithActors(
                It.IsAny<Film>(), It.IsAny<Guid?>(), It.IsAny<List<Guid>>()))
            .Verifiable(Times.Never);

        // Act
        var result = await _ctr.Edit(filmId, vm);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);

        _mockManager.VerifyAll();
    }

    #endregion
}