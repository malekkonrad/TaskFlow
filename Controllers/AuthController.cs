using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Models;

using TaskFlow.Services;

namespace TaskFlow.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
        // EnsureAdminExists();
    }


    [HttpGet]
    // GET: /Auth/Login
    public IActionResult Login()
    {
        return View();
    }


    // POST: /Auth/Login
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserName == username);
        var id = _context.Users.FirstOrDefault(u => u.UserName == username)?.Id;
        if (user != null && new PasswordService().VerifyPassword(password, user.Password))
        {
            HttpContext.Session.SetString("Id", id.ToString());
            HttpContext.Session.SetString("Username", user.UserName);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Nieprawidłowy login lub hasło.";
        return View();
    }


    [HttpGet]
    // GET: /Auth/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

}


