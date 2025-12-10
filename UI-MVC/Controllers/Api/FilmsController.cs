using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.UI.Web.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class FilmsController(IManager manager) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var books = manager.GetAllFilms();
        if (!books.Any())
            return NoContent();
        return Ok(books);
    }

    [Route("{id:guid}", Name = "GetFilm")]
    [HttpGet]
    public IActionResult Get(Guid id)
    {
        var film = manager.GetFilm(id);
        if (film == null)
            return NotFound();
        return Ok(film);
    }

    [HttpPost]
    public IActionResult Post(NewFilmDto newFilm)
    {
        Director director = null;
        if (newFilm.DirectorId.HasValue)
            director = manager.GetDirector(newFilm.DirectorId.Value);


        var addedFilm = manager.AddFilm(
            newFilm.Title,
            newFilm.Genre,
            newFilm.ReleaseDate,
            newFilm.Rating,
            director
        );

        return CreatedAtAction(nameof(Get), new { Id = addedFilm.ImdbId }, addedFilm);
    }
}