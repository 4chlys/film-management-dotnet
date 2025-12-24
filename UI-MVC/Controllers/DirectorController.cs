using FilmManagement.BL;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers;

public class DirectorController(IManager manager) : Controller
{
    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var director = manager.GetDirector(id);
        
        return View(director);
    }
}