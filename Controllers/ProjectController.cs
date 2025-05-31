using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[SessionAuthorize]
public class ProjectController : Controller
{
    private readonly AppDbContext _context;

    public ProjectController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Project
    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();
        var appDbContext = _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Where(p => p.OwnerId == currentUserId || p.Members.Any(m => m.UserId == currentUserId))
            .OrderByDescending(p => p.Id);
        return View(await appDbContext.ToListAsync());
    }

    // GET: Project/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Projects == null)
        {
            return NotFound();
        }

        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Assignee)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.Status)    
            .FirstOrDefaultAsync(m => m.Id == id);
        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    // GET: Project/Create
    public IActionResult Create()
    {
        ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName");
        return View();
    }

    // POST: Project/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,IsPublic")] Project project)
    {
        if (ModelState.IsValid)
        {
            project.OwnerId = GetCurrentUserId();

            Console.WriteLine($"Creating project: {project.Name}");
            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", project.OwnerId);
        Console.WriteLine("Model state is invalid, returning to create view.");
        return View(project);
    }

    // GET: Project/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Projects == null)
        {
            return NotFound();
        }

        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", project.OwnerId);
        return View(project);
    }

    // POST: Project/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsPublic,OwnerId")] Project project)
    {
        if (id != project.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "UserName", project.OwnerId);
        return View(project);
    }

    // GET: Project/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Projects == null)
        {
            return NotFound();
        }

        

        var project = await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (project == null)
        {
            return NotFound();
        }

        // Sprawdź czy aktualny użytkownik jest właścicielem projektu
        var currentUserId = GetCurrentUserId();
        if (project.OwnerId != currentUserId)
        {
            TempData["ErrorMessage"] = "Nie masz uprawnień do usunięcia tego projektu.";
            return RedirectToAction(nameof(Index));
        }

        return View(project);
    }

    // POST: Project/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Projects == null)
        {
            return Problem("Entity set 'AppDbContext.Projects'  is null.");
        }
        var project = await _context.Projects.FindAsync(id);

        // Sprawdź czy aktualny użytkownik jest właścicielem projektu
        var currentUserId = GetCurrentUserId();
        if (project != null && project.OwnerId != currentUserId)
        {
            TempData["ErrorMessage"] = "Nie masz uprawnień do usunięcia tego projektu.";
            return RedirectToAction(nameof(Index));
        }
        if (project != null)
        {
            _context.Projects.Remove(project);
        }

        

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    // project members
    // GET: Project/Members/5
    public async Task<IActionResult> Members(int? id)
    {
        if (id == null || _context.Projects == null)
        {
            return NotFound();
        }

        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
                .ThenInclude(pm => pm.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        // Sprawdź czy aktualny użytkownik jest właścicielem projektu
        var currentUserId = GetCurrentUserId();
        if (project.OwnerId != currentUserId)
        {
            TempData["ErrorMessage"] = "Nie masz uprawnień do usunięcia tego projektu.";
            return RedirectToAction(nameof(Index));
        }

        return View(project);
    }


    // GET: Project/AddMember/5
    public async Task<IActionResult> AddMember(int? id)
    {
        if (id == null || _context.Projects == null)
        {
            return NotFound();
        }

        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        

        // Sprawdź czy aktualny użytkownik jest właścicielem projektu
        var currentUserId = GetCurrentUserId();
        if (project.OwnerId != currentUserId)
        {
            TempData["ErrorMessage"] = "Nie masz uprawnień do usunięcia tego projektu.";
            return RedirectToAction(nameof(Index));
        }

        if (!project.IsPublic)
        {
            TempData["ErrorMessage"] = "Nie możesz dodawać członków do prywatnego projektu.";
            return RedirectToAction(nameof(Members), new { id = project.Id });
        }

        // Pobierz użytkowników którzy nie są jeszcze członkami projektu
        var existingMemberIds = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == id)
            .Select(pm => pm.UserId)
            .ToListAsync();

        var availableUsers = await _context.Users
            .Where(u => u.Id != project.OwnerId && !existingMemberIds.Contains(u.Id))
            .ToListAsync();

        ViewData["UserId"] = new SelectList(availableUsers, "Id", "UserName");
        ViewBag.ProjectId = id;
        ViewBag.ProjectName = project.Name;

        return View();
    }


    // POST: Project/AddMember
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember([Bind("ProjectId,UserId")] ProjectMember projectMember)
    {
        if (ModelState.IsValid)
        {
            // Sprawdź czy projekt istnieje i czy aktualny użytkownik jest właścicielem
            var project = await _context.Projects.FindAsync(projectMember.ProjectId);
            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (project.OwnerId != currentUserId)
            {
                TempData["ErrorMessage"] = "Nie masz uprawnień do dodawania członków do tego projektu.";
                return RedirectToAction(nameof(Index));
            }

            // Sprawdź czy użytkownik nie jest już członkiem projektu
            var existingMember = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectMember.ProjectId && pm.UserId == projectMember.UserId);

            if (existingMember != null)
            {
                ModelState.AddModelError("", "Ten użytkownik jest już członkiem projektu.");
                return RedirectToAction(nameof(AddMember), new { id = projectMember.ProjectId });
            }

            projectMember.JoinedAt = DateTime.UtcNow;
            _context.Add(projectMember);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Members), new { id = projectMember.ProjectId });
        }

        return RedirectToAction(nameof(AddMember), new { id = projectMember.ProjectId });
    }



    // POST: Project/RemoveMember
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int projectId, int userId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return NotFound();
        }

        // Sprawdź czy aktualny użytkownik jest właścicielem projektu
        var currentUserId = GetCurrentUserId();
        if (project.OwnerId != currentUserId)
        {
            TempData["ErrorMessage"] = "Nie masz uprawnień do usunięcia członka z tego projektu.";
            return RedirectToAction(nameof(Index));
        }

        var projectMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

        if (projectMember != null)
        {
            _context.ProjectMembers.Remove(projectMember);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Members), new { id = projectId });
    }







    private bool ProjectExists(int id)
    {
        return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    
    private int GetCurrentUserId()
    {
        var userId = HttpContext.Session.GetString("Id");
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not logged in.");
        }
        return int.Parse(userId);
    }


}

