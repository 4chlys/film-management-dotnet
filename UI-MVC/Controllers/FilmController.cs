using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.UI.Web.Models.Film;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmManagement.UI.Web.Controllers;

public class FilmController(IManager manager) : Controller
{
    public IActionResult Index()
    {
        var allFilms = manager.GetAllFilms();
        return View(allFilms);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new NewFilmViewModel
        {
            Directors = new SelectList(
                manager.GetAllDirectors(), 
                "ImdbId", 
                "Name"
            ),
            Actors = new SelectList(
                manager.GetAllActors(), 
                "ImdbId", 
                "Name"
            )
        };
        
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Create(NewFilmViewModel newFilm)
    {
        if (!ModelState.IsValid)
        {
            PopulateFilmSelectLists(newFilm);
            return View(newFilm);
        }
        
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

        if (newFilm.SelectedActorIds == null || newFilm.SelectedActorIds.Count == 0) return RedirectToAction("Details", new { Id = newFilm.ImdbId });
        foreach (var actorId in newFilm.SelectedActorIds)
        {
            try
            {
                manager.AddActorToFilm(addedFilm.ImdbId, actorId, null);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to add actor: {ex.Message}");
            }
        }

        return RedirectToAction("Details", new { Id = newFilm.ImdbId });
    }

    public IActionResult Details(Guid id)
    {
        var film = manager.GetFilmWithActorsAndDirectors(id);
    
        if (film == null)
        {
            return NotFound();
        }

        var detailFilm = new DetailsFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = GetGenreDisplayString(film.Genre),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            Director = film.Director != null 
                ? new DetailsFilmViewModel.DirectorInfo 
                { 
                    ImdbId = film.Director.ImdbId,
                    Name = film.Director.Name 
                }
                : null,
            Actors = film.ActorFilms?
                .Select(af => new DetailsFilmViewModel.ActorInfo
                {
                    ImdbId = af.Actor.ImdbId,
                    Name = af.Actor.Name,
                    DateOfBirth = af.Actor.DateOfBirth,
                    ScreenTime = af.ScreenTime
                })
                .ToList() ?? new()
        };
    
        return View(detailFilm);
    }


    [HttpGet]
    public IActionResult Edit(Guid id)
    {
        var film = manager.GetFilmWithActorsAndDirectors(id);
        
        var editFilm = new EditFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre,
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorId = film.Director?.ImdbId,
            SelectedActorIds = film.ActorFilms?.Select(af => af.Actor.ImdbId).ToList() ?? new(),
            Directors = new SelectList(
                manager.GetAllDirectors(),
                "ImdbId",
                "Name",
                film.Director?.ImdbId
            ),
            Actors = new SelectList(
                manager.GetAllActors(),
                "ImdbId",
                "Name"
            )
        };
        
        return View(editFilm);
    }

    [HttpPost]
    public IActionResult Edit(Guid id, EditFilmViewModel editFilm)
    {
        if (!ModelState.IsValid)
        {
            PopulateFilmSelectLists(editFilm);
            return View(editFilm);
        }
        
        var editExistingFilm = manager.GetFilm(id);
        
        if (editExistingFilm == null)
        {
            return NotFound();
        }
  
        editExistingFilm.Title = editFilm.Title;
        editExistingFilm.Genre = editFilm.Genre;
        editExistingFilm.ReleaseDate = editFilm.ReleaseDate;
        editExistingFilm.Rating = editFilm.Rating;
      
        editExistingFilm.Director = editFilm.DirectorId.HasValue ? manager.GetDirector(editFilm.DirectorId.Value) : null;
        
        manager.ChangeFilm(editExistingFilm);
    
        var existingActors = manager.GetActorsOfFilm(id).Select(a => a.ImdbId).ToList();
 
        foreach (var existingActorId in existingActors.Where(existingActorId => !editFilm.SelectedActorIds.Contains(existingActorId)))
        {
            try
            {
                manager.RemoveActorFromFilm(id, existingActorId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to remove actor: {ex.Message}");
            }
        }
        
        foreach (var actorId in editFilm.SelectedActorIds.Where(actorId => !existingActors.Contains(actorId)))
        {
            try
            {
                manager.AddActorToFilm(id, actorId, null);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to add actor: {ex.Message}");
            }
        }
        
        return RedirectToAction("Details", new { Id = editExistingFilm.ImdbId });
    }

    [HttpGet]
    public IActionResult Delete(Guid id)
    {
        var film = manager.GetFilm(id);
    
        if (film == null)
        {
            return NotFound();
        }

        var deleteFilm = new DeleteFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = GetGenreDisplayString(film.Genre),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorName = film.Director?.Name ?? "Unknown"
        };
    
        return View(deleteFilm);
    }

    [HttpPost]
    public IActionResult DeletePost(Guid id)
    {
        var film = manager.GetFilm(id);
    
        if (film == null)
        {
            return NotFound();
        }
    
        manager.RemoveFilm(film);
        return RedirectToAction("Index");
    }
    
    private void PopulateFilmSelectLists(BaseFilmFormViewModel filmFormViewModel)
    {
        filmFormViewModel.Directors = new SelectList(
            manager.GetAllDirectors(),
            "ImdbId",
            "Name",
            filmFormViewModel.DirectorId
        );
        filmFormViewModel.Actors = new SelectList(
            manager.GetAllActors(),
            "ImdbId",
            "Name"
        );
    }
    
    private string GetGenreDisplayString(Genre genre)
    {
        var genreList = Enum.GetValues(typeof(Genre))
            .Cast<Genre>()
            .Where(g => g != Genre.None && genre.HasFlag(g))
            .Select(g => g.ToString());

        return string.Join(", ", genreList);
    }
}