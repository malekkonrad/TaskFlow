using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using TaskFlow.Services;

[AdminAuthorize]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    // GET: Admin
    public IActionResult Index()
    {
        var users = _context.Users.ToList();
        return View(users); // Widok listy użytkowników
    }

    [HttpGet]
    // GET: Admin/AddUser
    public IActionResult AddUser()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddUser(User user, string password)
    {
        // Debugowanie wartości
        ViewBag.IsValid = ModelState.IsValid;
        ViewBag.Password = !string.IsNullOrEmpty(password) ? "Hasło podane" : "Brak hasła";
        ViewBag.RoleInSession = HttpContext.Session.GetString("Role");

        try
        {

            if (!string.IsNullOrEmpty(password))
            {
                user.Password = new PasswordService().HashPassword(password);
                user.ApiToken = Guid.NewGuid().ToString();
            }
            if (ModelState.IsValid)
            {
                // Wstaw wydruk sprawdzający obiekt
                System.Diagnostics.Debug.WriteLine($"UserName: {user.UserName}, Role: {user.Role}");

                _context.Users.Add(user);
                _context.SaveChanges();

                ViewBag.Success = "Użytkownik dodany pomyślnie";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.StackTrace = ex.StackTrace;
            ModelState.AddModelError("", "Błąd: " + ex.Message);
        }

        // Dodaj komunikat do widoku
        return View(user);
    }

    

    
}
