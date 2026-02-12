using FilmManagement.BL;
using FilmManagement.BL.Domain;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using FilmManagement.UI.Web.Models.Dto.Actor;
using FilmManagement.UI.Web.Models.Dto.Film;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class FilmsController(IManager manager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<FilmDto>> GetAll([FromQuery] Genre? genre = null)
    {
        IEnumerable<Film> films;
        
        if (genre.HasValue)
        {
            films = manager.GetFilmsByGenre(genre.Value);
        }
        else
        {
            films = manager.GetAllFilmsWithDirectors();
        }

        if (!films.Any())
            return NoContent();

        return Ok(films.Select(f => new FilmDto
        {
            ImdbId = f.ImdbId,
            Title = f.Title,
            Genre = f.Genre.ToString(),
            ReleaseDate = f.ReleaseDate,
            Rating = f.Rating,
            DirectorName = f.Director?.Name
        }));
    }

    [HttpGet("{filmId:guid}")]
    public ActionResult<FilmDto> Get(Guid filmId)
    {
        var film = manager.GetFilmWithDirector(filmId);
        if (film == null)
            return NotFound(new { error = "Film not found" });

        return Ok(new FilmDto
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre.ToString(),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorName = film.Director?.Name
        });
    }

    [HttpPost]
    public ActionResult<FilmDto> Create(NewFilmDto dto)
    {
        try
        {
            Director director = null;
            if (dto.DirectorId.HasValue)
            {
                director = manager.GetDirector(dto.DirectorId.Value);
                if (director == null)
                    return NotFound(new { error = $"Director with ID {dto.DirectorId.Value} not found" });
            }

            var film = manager.AddFilm(
                dto.Title,
                dto.Genre,
                dto.ReleaseDate,
                dto.Rating,
                director
            );

            return CreatedAtAction(
                nameof(Get),
                new { filmId = film.ImdbId },
                new FilmDto
                {
                    ImdbId = film.ImdbId,
                    Title = film.Title,
                    Genre = film.Genre.ToString(),
                    ReleaseDate = film.ReleaseDate,
                    Rating = film.Rating,
                    DirectorName = film.Director?.Name
                }
            );
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{filmId:guid}")]
    public IActionResult Update(Guid filmId, UpdateFilmDto dto)
    {
        try
        {
            var film = manager.GetFilm(filmId);
            if (film == null)
                return NotFound(new { error = "Film not found" });

            Director director = null;
            if (dto.DirectorId.HasValue)
            {
                director = manager.GetDirector(dto.DirectorId.Value);
                if (director == null)
                    return NotFound(new { error = $"Director with ID {dto.DirectorId.Value} not found" });
            }

            film.Title = dto.Title;
            film.Genre = dto.Genre;
            film.ReleaseDate = dto.ReleaseDate;
            film.Rating = dto.Rating;
            film.Director = director;

            manager.ChangeFilm(film);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{filmId:guid}")]
    public IActionResult Delete(Guid filmId)
    {
        var film = manager.GetFilm(filmId);
        if (film == null)
            return NotFound(new { error = "Film not found" });

        try
        {
            manager.RemoveFilm(film);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{filmId:guid}/actors")]
    public ActionResult<IEnumerable<ActorDto>> GetActorsByFilm(Guid filmId)
    {
        var film = manager.GetFilm(filmId);
        if (film == null)
            return NotFound(new { error = "Film not found" });

        var actors = manager.GetActorsOfFilm(filmId);
        if (!actors.Any())
            return NoContent();

        return Ok(actors.Select(a => new ActorDto
        {
            ImdbId = a.ImdbId,
            Name = a.Name,
            Nationality = a.Nationality,
            DateOfBirth = a.DateOfBirth,
            DateOfDeath = a.DateOfDeath,
            Age = a.Age
        }));
    }
}