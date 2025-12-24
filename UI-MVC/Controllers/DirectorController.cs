using FilmManagement.BL;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers;

public class DirectorController(IManager manager) : Controller
{
    [HttpGet]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Details(Guid id)
    {
        return View(manager.GetDirector(id));
    }

}