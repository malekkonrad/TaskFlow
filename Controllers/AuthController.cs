using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Models;


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
        if (user != null && VerifyPassword(password, user.Password))
        {
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


    private bool VerifyPassword(string inputPassword, string storedHash)
    {
        // W tym przykładzie zakładamy, że nie trzymamy soli w bazie, więc nie da się tak łatwo tego porównać.
        // W prawdziwej aplikacji przechowuj sól razem z hashem!
        return HashPassword(inputPassword) == storedHash;
    }

    private string HashPassword(string password)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }

    // private void EnsureAdminExists()
    // {
    //     if (!_context.Users.Any())
    //     {
    //         var admin = new User
    //         {
    //             UserName = "admin",
    //             Password = HashPassword("1234"),
    //             Role = "ADMIN",
    //             ApiToken = Guid.NewGuid().ToString()
    //         };

    //         _context.Users.Add(admin);
    //         _context.SaveChanges();
    //     }
    // }
}


