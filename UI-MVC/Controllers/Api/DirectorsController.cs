using FilmManagement.BL;
using FilmManagement.UI.Web.Models.Dto.Director;
using FilmManagement.UI.Web.Models.Dto.Film;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class DirectorsController(IManager manager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<DirectorDto>> GetAll()
    {
        var directors = manager.GetAllDirectors();
        if (!directors.Any())
        {
            return NoContent();
        }

        return Ok(
            directors.Select(d => new DirectorDto
            {
                ImdbId = d.ImdbId,
                Name = d.Name,
                Country = d.Country,
                CareerStart = d.CareerStart,
                CareerEnd = d.CareerEnd
            }));
    }

    [HttpGet("{directorId}")]
    public ActionResult<DirectorDto> Get(Guid directorId)
    {
        var director = manager.GetDirector(directorId);
        if (director == null)
        {
            return NotFound();
        }

        return Ok(new DirectorDto
        {
            ImdbId = director.ImdbId,
            Name = director.Name,
            Country = director.Country,
            CareerStart = director.CareerStart,
            CareerEnd = director.CareerEnd
        });
    }

    [HttpPost]
    public ActionResult<DirectorDto> Create(NewDirectorDto directorDto)
    {
        var createdDirector = manager.AddDirector(
            directorDto.Name,
            directorDto.Country,
            directorDto.CareerStart,
            directorDto.CareerEnd
        );
        var dto = new DirectorDto
        {
            ImdbId = createdDirector.ImdbId,
            Name = createdDirector.Name,
            Country = createdDirector.Country,
            CareerStart = createdDirector.CareerStart,
            CareerEnd = createdDirector.CareerEnd
        };
        return CreatedAtAction(
            nameof(Get),
            new { directorId = createdDirector.ImdbId },
            dto
        );
    }

    [HttpGet("{directorId}/films")]
    public ActionResult<IEnumerable<FilmDto>> GetFilmsByDirector(Guid directorId)
    {
        var films = manager.GetDirectorWithFilms(directorId)
            .Films;
        if (!films.Any())
        {
            return NoContent();
        }

        return Ok(
            films.Select(f => new FilmDto
            {
                ImdbId = f.ImdbId,
                Title = f.Title,
                Genre = f.Genre.ToString(),
                ReleaseDate = f.ReleaseDate,
                Rating = f.Rating,
                DirectorName = f.Director?.Name
            }));
    }
}