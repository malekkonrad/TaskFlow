using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[SessionAuthorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    // W HomeController.cs
    public IActionResult AccessDenied()
    {
        ViewBag.Message = HttpContext.Session.GetString("AccessDeniedMessage") ?? 
            "Nie masz uprawnień do wykonania tej operacji.";
        return View();
    }
}
