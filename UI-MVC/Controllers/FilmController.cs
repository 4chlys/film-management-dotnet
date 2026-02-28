using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.UI.Web.Controllers.Extensions;
using FilmManagement.UI.Web.Models;
using FilmManagement.UI.Web.Models.Film;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmManagement.UI.Web.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class FilmController(IManager manager, UserManager<IdentityUser> userManager) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Index()
    {
        var films = manager.GetAllFilmsWithDirectors();
        ViewBag.CurrentUserId = userManager.GetUserId(User);
        return View(films);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        PopulateSelectLists();
        return View(new NewFilmViewModel());
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(NewFilmViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateSelectLists();
            return View(model);
        }

        try
        {
            var currentUser = await userManager.GetUserAsync(User);

            var film = manager.AddFilmWithActors(
                model.Title,
                model.Genre,
                model.ReleaseDate,
                model.Rating,
                model.DirectorId,
                model.SelectedActorIds ?? new List<Guid>(),
                currentUser
            );
            return RedirectToAction(nameof(Details), new { id = film.ImdbId });
        }
        catch (ValidationException ex)
        {
            AddValidationErrors(ex.Message);
            PopulateSelectLists();
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            PopulateSelectLists();
            return View(model);
        }
    }
    
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var film = manager.GetFilmWithActorsAndDirectors(id);
        if (film == null)
        {
            return View("Error", new ErrorViewModel(
                404,
                "Film Not Found",
                $"The film with ID '{id}' could not be found. It may have been deleted or never existed."
            ));
        }

        var vm = MapToDetailsViewModel(film);
     
        if (TempData["ErrorMessage"] != null)
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var film = manager.GetFilmWithActorsAndDirectors(id);
        
        if (film == null)
        {
            return View("Error", new ErrorViewModel(
                404,
                "Film Not Found",
                $"Cannot edit film with ID '{id}' because it does not exist."
            ));
        }

        var currentUser = await userManager.GetUserAsync(User);
    
        if (film.Maintainer?.Id != currentUser?.Id && !User.IsInRole("Admin"))
            return RedirectToPage("/Account/AccessDenied", new { Area = "Identity" });

        var vm = MapToEditViewModel(film);
        PopulateSelectLists(vm.DirectorId);
        return View(vm);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, EditFilmViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            PopulateSelectLists(vm.DirectorId);
            return View(vm);
        }

        try
        {
            var film = manager.GetFilm(id);
            if (film == null)
            {
                return View("Error", new ErrorViewModel(
                    404, "Film Not Found",
                    $"Cannot edit film with ID '{id}' because it does not exist."
                ));
            }

            var currentUser = await userManager.GetUserAsync(User);

            if (film.Maintainer?.Id != currentUser?.Id && !User.IsInRole("Admin"))
                return RedirectToPage("/Account/AccessDenied", new { Area = "Identity" });

            film.Title = vm.Title;
            film.Genre = vm.Genre;
            film.ReleaseDate = vm.ReleaseDate;
            film.Rating = vm.Rating;

            manager.ChangeFilmWithActors(
                film,
                vm.DirectorId,
                vm.SelectedActorIds ?? new List<Guid>()
            );
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ValidationException ex)
        {
            AddValidationErrors(ex.Message);
            PopulateSelectLists(vm.DirectorId);
            return View(vm);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            PopulateSelectLists(vm.DirectorId);
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult Delete(Guid id)
    {
        var film = manager.GetFilm(id);
        if (film == null)
        {
            return View("Error", new ErrorViewModel(
                404,
                "Film Not Found",
                $"Cannot delete film with ID '{id}' because it does not exist."
            ));
        }

        var vm = MapToDeleteViewModel(film);
        return View(vm);
    }

    [HttpPost]
    public IActionResult DeletePost(Guid id)
    {
        try
        {
            var film = manager.GetFilm(id);
            if (film == null)
            {
                return View("Error", new ErrorViewModel(
                    404,
                    "Film Not Found",
                    $"Cannot delete film with ID '{id}' because it does not exist."
                ));
            }

            manager.RemoveFilm(film);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    public IActionResult UpdateScreenTime(Guid filmId, Guid actorId, int? screenTime)
    {
        try
        {
            manager.UpdateActorFilmScreenTime(filmId, actorId, screenTime);
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = $"Validation error: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = filmId });
    }

    private DetailsFilmViewModel MapToDetailsViewModel(BL.Domain.Film film)
    {
        return new DetailsFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre.ToDisplayString(),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            MaintainerName = film.Maintainer?.UserName,
            Director = film.Director == null ? null : new DetailsFilmViewModel.DirectorInfo
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
                .ToList() ?? []
        };
    }

    private EditFilmViewModel MapToEditViewModel(BL.Domain.Film film)
    {
        return new EditFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre,
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorId = film.Director?.ImdbId,
            SelectedActorIds = film.ActorFilms?
                .Select(af => af.Actor.ImdbId)
                .ToList() ?? []
        };
    }

    private DeleteFilmViewModel MapToDeleteViewModel(BL.Domain.Film film)
    {
        return new DeleteFilmViewModel
        {
            ImdbId = film.ImdbId,
            Title = film.Title,
            Genre = film.Genre.ToDisplayString(),
            ReleaseDate = film.ReleaseDate,
            Rating = film.Rating,
            DirectorName = film.Director?.Name ?? "Unknown"
        };
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

    private void AddValidationErrors(string errorMessage)
    {
        var errors = errorMessage.Split('|', StringSplitOptions.RemoveEmptyEntries);
        foreach (var error in errors)
        {
            ModelState.AddModelError("", error.Trim());
        }
    }
}