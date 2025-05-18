using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[SessionAuthorize]
public class ProjectController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
