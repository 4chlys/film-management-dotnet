using FilmManagement.BL;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers;

public class ActorController(IManager manager) : Controller
{
    public IActionResult Details(Guid id)
    {
        var actor = manager.GetActor(id);
        
        return View(actor);
    }
}