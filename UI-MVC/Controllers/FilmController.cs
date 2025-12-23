using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.UI.Web.Models.Film;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmManagement.UI.Web.Controllers;

public class FilmController(IManager manager) : Controller
{
    /* -------------------- INDEX -------------------- */

    public IActionResult Index()
    {
        var films = manager.GetAllFilmsWithDirectors();
        return View(films);
    }

    /* -------------------- CREATE -------------------- */

    [HttpGet]
    public IActionResult Create()
    {
        PopulateSelectLists();
        return View(new NewFilmViewModel());
    }

    [HttpPost]
    public IActionResult Create(NewFilmViewModel model)
    {
        if (!ModelState.IsValid)
            return ReturnCreateView(model);

        var actorErrors = ValidateActorIds(model.SelectedActorIds);
        var director = ValidateDirectorId(model.DirectorId, out var directorError);

        AddErrors("SelectedActorIds", actorErrors);
        AddError("DirectorId", directorError);

        if (!ModelState.IsValid)
            return ReturnCreateView(model);

        try
        {
            var film = manager.AddFilm(
                model.Title,
                model.Genre,
                model.ReleaseDate,
                model.Rating,
                director
            );

            AddActorsToFilm(film.ImdbId, model.SelectedActorIds);

            return RedirectToAction("Details", new { id = film.ImdbId });
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ReturnCreateView(model);
        }
    }

    /* -------------------- DETAILS -------------------- */

    public IActionResult Details(Guid id)
    {
        var film = manager.GetFilmWithActorsAndDirectors(id);
        if (film == null)
            return NotFound();

        var vm = new DetailsFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = GetGenreDisplayString(film.Genre),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            Director = film.Director == null
                ? null
                : new DetailsFilmViewModel.DirectorInfo
                {
                    ImdbId = film.Director.ImdbId,
                    Name = film.Director.Name
                },
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

        return View(vm);
    }

    /* -------------------- EDIT -------------------- */

    [HttpGet]
    public IActionResult Edit(Guid id)
    {
        var film = manager.GetFilmWithActorsAndDirectors(id);
        if (film == null)
            return NotFound();

        var model = new EditFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre,
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorId = film.Director?.ImdbId,
            SelectedActorIds = film.ActorFilms?
                .Select(af => af.Actor.ImdbId)
                .ToList() ?? new()
        };

        PopulateSelectLists(model.DirectorId);
        return View(model);
    }

    [HttpPost]
    public IActionResult Edit(Guid id, EditFilmViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateSelectLists(model.DirectorId);
            return View(model);
        }

        var actorErrors = ValidateActorIds(model.SelectedActorIds);
        var director = ValidateDirectorId(model.DirectorId, out var directorError);

        AddErrors("SelectedActorIds", actorErrors);
        AddError("DirectorId", directorError);

        if (!ModelState.IsValid)
        {
            PopulateSelectLists(model.DirectorId);
            return View(model);
        }

        var film = manager.GetFilm(id);
        if (film == null)
            return NotFound();

        film.Title = model.Title;
        film.Genre = model.Genre;
        film.ReleaseDate = model.ReleaseDate;
        film.Rating = model.Rating;
        film.Director = director;

        manager.ChangeFilm(film);

        SyncActors(id, model.SelectedActorIds);

        return RedirectToAction("Details", new { id });
    }

    /* -------------------- DELETE -------------------- */

    [HttpGet]
    public IActionResult Delete(Guid id)
    {
        var film = manager.GetFilm(id);
        if (film == null)
            return NotFound();

        return View(new DeleteFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = GetGenreDisplayString(film.Genre),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorName = film.Director?.Name ?? "Unknown"
        });
    }

    [HttpPost]
    public IActionResult DeletePost(Guid id)
    {
        var film = manager.GetFilm(id);
        if (film == null)
            return NotFound();

        manager.RemoveFilm(film);
        return RedirectToAction("Index");
    }

    /* -------------------- HELPERS -------------------- */

    private List<string> ValidateActorIds(IEnumerable<Guid>? actorIds)
    {
        var errors = new List<string>();

        if (actorIds == null)
            return errors;

        foreach (var actorId in actorIds)
        {
            if (manager.GetActor(actorId) == null)
                errors.Add($"Actor with ID {actorId} not found.");
        }

        return errors;
    }

    private Director? ValidateDirectorId(Guid? directorId, out string? error)
    {
        error = null;

        if (!directorId.HasValue)
            return null;

        var director = manager.GetDirector(directorId.Value);
        if (director == null)
        {
            error = "Selected director not found.";
            return null;
        }

        return director;
    }

    private void AddActorsToFilm(Guid filmId, IEnumerable<Guid>? actorIds)
    {
        if (actorIds == null)
            return;

        foreach (var actorId in actorIds)
            manager.AddActorToFilm(filmId, actorId, null);
    }

    private void SyncActors(Guid filmId, List<Guid> selectedActorIds)
    {
        var existing = manager.GetActorsOfFilm(filmId)
            .Select(a => a.ImdbId)
            .ToHashSet();

        foreach (var actorId in existing.Except(selectedActorIds))
            manager.RemoveActorFromFilm(filmId, actorId);

        foreach (var actorId in selectedActorIds.Except(existing))
            manager.AddActorToFilm(filmId, actorId, null);
    }

    private void AddErrors(string key, IEnumerable<string> errors)
    {
        foreach (var error in errors)
            ModelState.AddModelError(key, error);
    }

    private void AddError(string key, string? error)
    {
        if (error != null)
            ModelState.AddModelError(key, error);
    }

    private IActionResult ReturnCreateView(NewFilmViewModel model)
    {
        PopulateSelectLists();
        return View(model);
    }

    private void PopulateSelectLists(Guid? selectedDirectorId = null)
    {
        ViewBag.Directors = new SelectList(
            manager.GetAllDirectors(),
            "ImdbId",
            "Name",
            selectedDirectorId
        );

        ViewBag.Actors = new SelectList(
            manager.GetAllActors(),
            "ImdbId",
            "Name"
        );
    }

    private string GetGenreDisplayString(Genre genre)
    {
        return string.Join(", ",
            Enum.GetValues<Genre>()
                .Where(g => g != Genre.None && genre.HasFlag(g))
        );
    }
}
