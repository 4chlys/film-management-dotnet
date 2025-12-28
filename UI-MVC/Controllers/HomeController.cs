using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FilmManagement.UI.Web.Models;

namespace FilmManagement.UI.Web.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        if (!statusCode.HasValue) return View(errorViewModel);
        errorViewModel.StatusCode = statusCode.Value;
            
        switch (statusCode.Value)
        {
            case 404:
                errorViewModel.Title = "Page Not Found";
                errorViewModel.Message = "The page you're looking for doesn't exist.";
                break;
            case 403:
                errorViewModel.Title = "Access Denied";
                errorViewModel.Message = "You don't have permission to access this resource.";
                break;
            case 400:
                errorViewModel.Title = "Bad Request";
                errorViewModel.Message = "The request could not be understood by the server.";
                break;
            default:
                errorViewModel.Title = "Error";
                errorViewModel.Message = "An error occurred while processing your request.";
                break;
        }

        return View(errorViewModel);
    }
}