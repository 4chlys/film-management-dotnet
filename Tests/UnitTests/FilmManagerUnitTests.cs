using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.DAL;
using Moq;

namespace FilmManagement.UI.MVC.Tests.UnitTests;

public class FilmManagerUnitTests
{
    private readonly Mock<IRepository> _mock = new();
    private readonly Manager _mgr;

    public FilmManagerUnitTests()
    {
        _mgr = new Manager(_mock.Object);
    }
    
    #region AddFilm

    [Fact]
    public void AddFilm_ReturnsFilm_CallsRepositoryOnce_GivenValidData()
    {
        // Arrange
        string title = "Inception";
        Genre genre = Genre.SciFi;
        DateTime releaseDate = new DateTime(2010, 7, 16);
        double rating = 8.8;

        _mock.Setup(repo => repo.CreateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Exactly(1));

        // Act
        var result = _mgr.AddFilm(title, genre, releaseDate, rating, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(genre, result.Genre);
        Assert.Equal(rating, result.Rating);

        _mock.VerifyAll();
    }

    [Fact]
    public void AddFilm_ThrowsValidationException_DoesNotCallRepository_GivenTitleTooShort()
    {
        // Arrange
        _mock.Setup(repo => repo.CreateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Never);

        // Act
        var exception = Record.Exception(() =>
            _mgr.AddFilm("AB", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);

        _mock.VerifyAll();
    }

    [Fact]
    public void AddFilm_ThrowsValidationException_DoesNotCallRepository_GivenEmptyTitle()
    {
        // Arrange
        _mock.Setup(repo => repo.CreateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Never);

        // Act
        var exception = Record.Exception(() =>
            _mgr.AddFilm("", Genre.Drama, new DateTime(2020, 1, 1), 7.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);

        _mock.VerifyAll();
    }

    [Fact]
    public void AddFilm_ThrowsValidationException_DoesNotCallRepository_GivenFutureReleaseDate()
    {
        // Arrange
        _mock.Setup(repo => repo.CreateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Never);

        // Act
        var exception = Record.Exception(() =>
            _mgr.AddFilm("Valid Title", Genre.Drama, DateTime.Now.AddYears(1), 7.0, null, null));

        // Assert
        Assert.IsType<ValidationException>(exception);

        _mock.VerifyAll();
    }

    #endregion

    #region ChangeFilm

    [Fact]
    public void ChangeFilm_CallsRepositoryOnce_GivenValidFilm()
    {
        // Arrange
        var film = new Film
        {
            ImdbId = Guid.NewGuid(),
            Title = "Updated Title",
            Genre = Genre.Action,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 8.0
        };

        _mock.Setup(repo => repo.UpdateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Exactly(1));

        // Act
        _mgr.ChangeFilm(film);

        // Assert
        _mock.VerifyAll();
    }

    [Fact]
    public void ChangeFilm_ThrowsValidationException_DoesNotCallRepository_GivenTitleTooShort()
    {
        // Arrange
        var film = new Film
        {
            ImdbId = Guid.NewGuid(),
            Title = "AB",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.0
        };

        _mock.Setup(repo => repo.UpdateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Never);

        // Act
        var exception = Record.Exception(() => _mgr.ChangeFilm(film));

        // Assert
        Assert.IsType<ValidationException>(exception);

        _mock.VerifyAll();
    }

    [Fact]
    public void ChangeFilm_ThrowsValidationException_DoesNotCallRepository_GivenEmptyTitle()
    {
        // Arrange
        var film = new Film
        {
            ImdbId = Guid.NewGuid(),
            Title = "",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(2020, 1, 1),
            Rating = 7.0
        };

        _mock.Setup(repo => repo.UpdateFilm(It.IsAny<Film>()))
            .Verifiable(Times.Never);

        // Act
        var exception = Record.Exception(() => _mgr.ChangeFilm(film));

        // Assert
        Assert.IsType<ValidationException>(exception);

        _mock.VerifyAll();
    }

    #endregion
}