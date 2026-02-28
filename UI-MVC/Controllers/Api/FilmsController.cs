using FilmManagement.BL;
using FilmManagement.BL.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using FilmManagement.UI.Web.Models.Dto.Actor;
using FilmManagement.UI.Web.Models.Dto.Film;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class FilmsController(IManager manager, UserManager<IdentityUser> userManager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<FilmDto>> GetAll([FromQuery] Genre? genre = null)
    {
        IEnumerable<Film> films;

        if (genre.HasValue)
            films = manager.GetFilmsByGenre(genre.Value);
        else
            films = manager.GetAllFilmsWithDirectors();

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

    [Authorize]
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
                director,
                null
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

    [Authorize]
    [HttpPut("{filmId:guid}")]
    public async Task<IActionResult> Update(Guid filmId, UpdateFilmDto dto)
    {
        try
        {
            var film = manager.GetFilm(filmId);
            if (film == null)
                return NotFound(new { error = "Film not found" });

            var currentUser = await userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (film.Maintainer?.Id != currentUser?.Id && !isAdmin)
                return StatusCode(403, new { error = "You are not authorized to edit this film." });

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
    
    [Authorize]
    [HttpPut("{filmId:guid}/rating")]
    public async Task<IActionResult> UpdateRating(Guid filmId, [FromBody] double rating)
    {
        if (rating < 0 || rating > 10)
            return BadRequest(new { error = "Rating must be between 0 and 10." });

        var film = manager.GetFilm(filmId);
        if (film == null)
            return NotFound(new { error = "Film not found" });

        var currentUser = await userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin");

        if (film.Maintainer?.Id != currentUser?.Id && !isAdmin)
            return StatusCode(403, new { error = "You are not the maintainer of this film." });

        film.Rating = rating;

        try
        {
            manager.ChangeFilm(film);
            return Ok(new { rating = film.Rating });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
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