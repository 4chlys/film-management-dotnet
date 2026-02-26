using FilmManagement.BL;
using FilmManagement.UI.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmManagement.UI.Web.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class DirectorController(IManager manager) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var director = manager.GetDirector(id);
        if (director == null)
        {
            return View("Error", new ErrorViewModel(
                404,
                "Director Not Found",
                $"The director with ID '{id}' could not be found. It may have been deleted or never existed."
            ));
        }

        return View(director);
    }
}