using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.UI.Web.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class FilmsController(IManager manager) : ControllerBase
{
    [HttpGet("{filmId:guid}")]
    public ActionResult<FilmDto> Get(Guid filmId)
    {
        var film = manager.GetFilm(filmId);
        if (film == null)
            return NotFound();

        return Ok(new FilmDto
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre.ToString(),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorName = film.Director.Name
        });
    }

    [HttpPost]
    public ActionResult<FilmDto> Create(NewFilmDto dto)
    {
        Director director = null;

        if (dto.DirectorId.HasValue)
        {
            director = manager.GetDirector(dto.DirectorId.Value);
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


    [HttpPut("{filmId:guid}")]
    public IActionResult Update(Guid filmId, UpdateFilmDto dto)
    {
        Director director = null;

        if (dto.DirectorId.HasValue)
        {
            director = manager.GetDirector(dto.DirectorId.Value);
        }

        var filmToChange = new Film
        {
            ImdbId = filmId,
            Title = dto.Title,
            Genre = dto.Genre,
            ReleaseDate = dto.ReleaseDate,
            Rating = dto.Rating,
            Director = director
        };
        manager.ChangeFilm(filmToChange);

        return NoContent();
    }

    [HttpDelete("{filmId:guid}")]
    public IActionResult Delete(Guid filmId)
    {
        var filmToDelete = manager.GetFilm(filmId);
        if (filmToDelete == null)
            return NotFound();

        manager.RemoveFilm(filmToDelete);

        return NoContent();
    }
}