using FilmManagement.BL;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using FilmManagement.UI.Web.Models.Dto.Actor;
using FilmManagement.UI.Web.Models.Dto.Film;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ActorsController(IManager manager) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<ActorDto>> GetAll()
    {
        var actors = manager.GetAllActors();
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

    [HttpGet("{actorId:guid}")]
    public ActionResult<ActorDto> Get(Guid actorId)
    {
        var actor = manager.GetActor(actorId);
        if (actor == null)
            return NotFound(new { error = "Actor not found" });

        return Ok(new ActorDto
        {
            ImdbId = actor.ImdbId,
            Name = actor.Name,
            Nationality = actor.Nationality,
            DateOfBirth = actor.DateOfBirth,
            DateOfDeath = actor.DateOfDeath,
            Age = actor.Age
        });
    }

    [HttpPost]
    public ActionResult<ActorDto> Create(NewActorDto dto)
    {
        try
        {
            var actor = manager.AddActor(
                dto.Name,
                dto.Nationality,
                dto.DateOfBirth,
                dto.DateOfDeath
            );

            var actorDto = new ActorDto
            {
                ImdbId = actor.ImdbId,
                Name = actor.Name,
                Nationality = actor.Nationality,
                DateOfBirth = actor.DateOfBirth,
                DateOfDeath = actor.DateOfDeath,
                Age = actor.Age
            };

            return CreatedAtAction(
                nameof(Get),
                new { actorId = actor.ImdbId },
                actorDto
            );
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{actorId:guid}")]
    public IActionResult Update(Guid actorId, UpdateActorDto dto)
    {
        try
        {
            var actor = manager.GetActor(actorId);
            if (actor == null)
                return NotFound(new { error = "Actor not found" });

            actor.Name = dto.Name;
            actor.Nationality = dto.Nationality;
            actor.DateOfBirth = dto.DateOfBirth;
            actor.DateOfDeath = dto.DateOfDeath;

            manager.ChangeActor(actor);
            return Ok(actor);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{actorId:guid}")]
    public IActionResult Delete(Guid actorId)
    {
        var actor = manager.GetActor(actorId);
        if (actor == null)
            return NotFound(new { error = "Actor not found" });

        try
        {
            manager.RemoveActor(actor);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{actorId:guid}/films")]
    public ActionResult<IEnumerable<FilmWithScreenTimeDto>> GetFilmsByActor(Guid actorId)
    {
        var actor = manager.GetActorWithFilms(actorId);
        if (actor == null)
            return NotFound(new { error = "Actor not found" });

        var actorFilms = actor.ActorFilms;
        if (!actorFilms.Any())
            return NoContent();

        return Ok(actorFilms.Select(af => new FilmWithScreenTimeDto
        {
            ImdbId = af.Film.ImdbId,
            Title = af.Film.Title,
            Genre = af.Film.Genre.ToString(),
            ReleaseDate = af.Film.ReleaseDate,
            Rating = af.Film.Rating,
            DirectorName = af.Film.Director?.Name,
            ScreenTime = af.ScreenTime
        }));
    }
}