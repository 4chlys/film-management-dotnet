using FilmManagement.BL;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers;

public class ActorController(IManager manager) : Controller
{
    [HttpGet]
    public IActionResult Details(Guid id)
    {
        return View(manager.GetActor(id));
    }
}