using FilmManagement.BL;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using FilmManagement.UI.Web.Models.Dto.ActorFilm;

namespace FilmManagement.UI.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ActorFilmsController(IManager manager) : ControllerBase
{
    [HttpPost]
    public ActionResult Create(NewActorFilmDto dto)
    {
        try
        {
            manager.AddActorToFilm(dto.FilmId, dto.ActorId, dto.ScreenTime);
            
            return CreatedAtAction(
                nameof(Get),
                new { filmId = dto.FilmId, actorId = dto.ActorId },
                new ActorFilmDto
                {
                    FilmId = dto.FilmId,
                    ActorId = dto.ActorId,
                    ScreenTime = dto.ScreenTime
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{filmId:guid}/{actorId:guid}")]
    public ActionResult<ActorFilmDto> Get(Guid filmId, Guid actorId)
    {
        try
        {
            var actorFilm = manager.GetActorsOfFilm(filmId)
                .FirstOrDefault(a => a.ImdbId == actorId);

            if (actorFilm == null)
                return NotFound(new { error = "Actor-Film relationship not found" });

            var film = manager.GetFilmWithActorsAndDirectors(filmId);
            var af = film.ActorFilms.FirstOrDefault(af => af.Actor.ImdbId == actorId);

            return Ok(new ActorFilmDto
            {
                FilmId = filmId,
                ActorId = actorId,
                ScreenTime = af?.ScreenTime
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{filmId:guid}/{actorId:guid}")]
    public IActionResult UpdateScreenTime(Guid filmId, Guid actorId, [FromBody] int? screenTime)
    {
        try
        {
            manager.UpdateActorFilmScreenTime(filmId, actorId, screenTime);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{filmId:guid}/{actorId:guid}")]
    public IActionResult Delete(Guid filmId, Guid actorId)
    {
        try
        {
            manager.RemoveActorFromFilm(filmId, actorId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}