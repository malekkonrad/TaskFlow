using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using TaskFlow.Services;
using Microsoft.EntityFrameworkCore;

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


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Użytkownik nie został znaleziony.";
                return RedirectToAction("Index");
            }

            // Sprawdź czy to nie jest jedyny admin
            var adminCount = await _context.Users.CountAsync(u => u.Role == "ADMIN");
            if (user.Role == "ADMIN" && adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Nie można usunąć jedynego administratora w systemie.";
                return RedirectToAction("Index");
            }

            // Sprawdź czy użytkownik nie jest właścicielem projektów
            // var ownedProjectsCount = await _context.Projects.CountAsync(p => p.OwnerId == id);
            // if (ownedProjectsCount > 0)
            // {
            //     TempData["ErrorMessage"] = $"Nie można usunąć użytkownika, który jest właścicielem {ownedProjectsCount} projektów. Najpierw przenieś własność projektów.";
            //     return RedirectToAction("Index");
            // }

            // Usuń użytkownika z członkostwa w projektach
            var projectMemberships = await _context.ProjectMembers
                .Where(pm => pm.UserId == id)
                .ToListAsync();
            
            if (projectMemberships.Any())
            {
                _context.ProjectMembers.RemoveRange(projectMemberships);
            }

            // Usuń komentarze użytkownika (lub ustaw AuthorId na null)
            var userComments = await _context.Comments
                .Where(c => c.AuthorId == id)
                .ToListAsync();
            
            foreach (var comment in userComments)
            {
                comment.AuthorId = null; // Pozostaw komentarz ale usuń powiązanie z autorem
            }

            // Usuń przypisania tasków
            var assignedTasks = await _context.Tasks
                .Where(t => t.AssigneeId == id)
                .ToListAsync();
            
            foreach (var task in assignedTasks)
            {
                task.AssigneeId = null; // Ustaw jako nieprzypisane
            }

            // Usuń użytkownika
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Użytkownik {user.UserName} został pomyślnie usunięty.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Błąd podczas usuwania użytkownika: {ex.Message}";
        }

        return RedirectToAction("Index");
    }





    

    
}
