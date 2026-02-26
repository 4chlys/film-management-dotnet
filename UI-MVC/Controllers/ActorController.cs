using FilmManagement.BL;
using FilmManagement.UI.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class ActorController(IManager manager) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var actor = manager.GetActor(id);
        if (actor == null)
        {
            return View("Error", new ErrorViewModel(
                404,
                "Actor Not Found",
                $"The actor with ID '{id}' could not be found. It may have been deleted or never existed."
            ));
        }

        return View(actor);
    }
}