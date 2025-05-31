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
public class UserTaskController : Controller
{
    private readonly AppDbContext _context;

    public UserTaskController(AppDbContext context)
    {
        _context = context;
    }

    // GET: UserTask
    public async Task<IActionResult> Index()
    {
        var appDbContext = _context.Tasks.Include(u => u.Assignee).Include(u => u.Project).Include(u => u.Status).Where(u=> u.AssigneeId == GetCurrentUserId());
        return View(await appDbContext.ToListAsync());
    }

    // GET: UserTask/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Tasks == null)
        {
            return NotFound();
        }

        var userTask = await _context.Tasks
            .Include(u => u.Assignee)
            .Include(u => u.Project)
            .Include(u => u.Status)
            .Include(u => u.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (userTask == null)
        {
            return NotFound();
        }

        return View(userTask);
    }

    // // GET: UserTask/Create
    // public IActionResult Create(int? projectId)
    // {

    //     var currentUserId = GetCurrentUserId();

    //     var users = new List<User>();
    //     if (projectId.HasValue)
    //     {
    //         // Get users that are members of the specified project
    //         var projectMembers = _context.ProjectMembers
    //             .Where(pm => pm.ProjectId == projectId)
    //             .Select(pm => pm.User)
    //             .ToList();
    //         users.AddRange(projectMembers);

    //         users.AddRange(_context.Users
    //             .Where(u => u.OwnedProjects.Any(p => p.Id == projectId.Value))
    //             .ToList());
    //     }
    //     else
    //     {
    //         // If no project specified, use all users
    //         users = _context.Users.ToList();
    //     }

    //     // Create item for "Unassigned" option
    //     var unassignedItem = new SelectListItem { Value = "", Text = "-- Unassigned --" };
    //     var selectItems = users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Id == currentUserId ? "-- Me --" : u.UserName }).ToList();
    //     selectItems.Insert(0, unassignedItem);  // Add unassigned at the beginning

    //     ViewData["AssigneeId"] = new SelectList(selectItems, "Value", "Text");
    //     if (projectId.HasValue)
    //     {
    //         // Jeśli mamy projectId, ustaw go jako wybrany i pokaż tylko ten projekt
    //         ViewData["ProjectId"] = new SelectList(_context.Projects.Where(p => p.Id == projectId), "Id", "Name", projectId);
    //         ViewData["SelectedProjectId"] = projectId;
    //     }
    //     else
    //     {
    //         // Jeśli nie ma projectId, pokaż wszystkie projekty
    //         ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
    //     }

    //     ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name");
    //     return View();
    // }


    // GET: UserTask/Create
    public IActionResult Create(int? projectId, int? selectedProjectId)
    {
        var currentUserId = GetCurrentUserId();
        
        // Pobierz wszystkie projekty użytkownika
        var userProjects = _context.Projects
            .Where(p => p.OwnerId == currentUserId || p.Members.Any(m => m.UserId == currentUserId))
            .ToList();
        
        ViewBag.ProjectId = new SelectList(userProjects, "Id", "Name", selectedProjectId ?? projectId);
        ViewBag.StatusId = new SelectList(_context.Statuses, "Id", "Name");
        
        // KLUCZOWA CZĘŚĆ: Jeśli wybrano projekt (selectedProjectId lub projectId)
        if (selectedProjectId.HasValue || projectId.HasValue)
        {
            var chosenProjectId = selectedProjectId ?? projectId;
            
            // Pobierz członków wybranego projektu
            var projectMembers = _context.ProjectMembers
                .Where(pm => pm.ProjectId == chosenProjectId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToList();
            
            // Pobierz właściciela projektu
            var projectOwner = _context.Projects
                .Where(p => p.Id == chosenProjectId)
                .Include(p => p.Owner)
                .Select(p => p.Owner)
                .FirstOrDefault();
            
            var users = new List<User>();
            if (projectOwner != null) users.Add(projectOwner);
            users.AddRange(projectMembers);
            
            // Usuń duplikaty
            users = users.GroupBy(u => u.Id).Select(g => g.First()).ToList();
            
            // Przygotuj listę assignee
            var unassignedItem = new SelectListItem { Value = "", Text = "-- Unassigned --" };
            var selectItems = users.Select(u => new SelectListItem { 
                Value = u.Id.ToString(), 
                Text = u.Id == currentUserId ? "-- Me --" : u.UserName 
            }).ToList();
            selectItems.Insert(0, unassignedItem);
            
            ViewData["AssigneeId"] = new SelectList(selectItems, "Value", "Text");
            ViewData["SelectedProjectId"] = chosenProjectId; // Oznacz że projekt został wybrany
        }
        else
        {
            // Brak wybranego projektu - pusta lista assignee
            ViewData["AssigneeId"] = new SelectList(new List<SelectListItem>(), "Value", "Text");
        }
        
        return View();
    }

    // NOWA AKCJA: Obsługuje tylko wybór projektu
    [HttpPost]
    public IActionResult SelectProject(int projectId)
    {
        // Przekieruj z powrotem do Create z wybranym projektem
        return RedirectToAction("Create", new { selectedProjectId = projectId });
    }
    










    // POST: UserTask/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,Description,Deadline,ProjectId,AssigneeId,StatusId")] UserTask userTask)
    {
        if (ModelState.IsValid)
        {
            userTask.CreatedAt = DateTime.Now; // Ustawienie daty utworzenia na teraz

            _context.Add(userTask);
            await _context.SaveChangesAsync();
            // Po utworzeniu taska, przekieruj z powrotem do szczegółów projektu
            return RedirectToAction("Details", "Project", new { id = userTask.ProjectId });
        }
        ViewData["AssigneeId"] = new SelectList(_context.Users, "Id", "UserName", userTask.AssigneeId);
        ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", userTask.ProjectId);
        ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name", userTask.StatusId);
        return View(userTask);
    }

    // GET: UserTask/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Tasks == null)
        {
            return NotFound();
        }

        var userTask = await _context.Tasks.FindAsync(id);
        if (userTask == null)
        {
            return NotFound();
        }
        ViewData["AssigneeId"] = new SelectList(_context.Users, "Id", "UserName", userTask.AssigneeId);
        ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", userTask.ProjectId);
        ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name", userTask.StatusId);
        return View(userTask);
    }

    // POST: UserTask/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedAt,Deadline,ProjectId,AssigneeId,StatusId")] UserTask userTask)
    {
        if (id != userTask.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(userTask);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTaskExists(userTask.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Details", "Project", new { id = userTask.ProjectId });
        }
        ViewData["AssigneeId"] = new SelectList(_context.Users, "Id", "UserName", userTask.AssigneeId);
        ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", userTask.ProjectId);
        ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name", userTask.StatusId);
        return View(userTask);
    }

    // GET: UserTask/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Tasks == null)
        {
            return NotFound();
        }

        var userTask = await _context.Tasks
            .Include(u => u.Assignee)
            .Include(u => u.Project)
            .Include(u => u.Status)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (userTask == null)
        {
            return NotFound();
        }

        return View(userTask);
    }

    // POST: UserTask/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Tasks == null)
        {
            return Problem("Entity set 'AppDbContext.Tasks'  is null.");
        }
        var userTask = await _context.Tasks.FindAsync(id);
        if (userTask != null)
        {
            _context.Tasks.Remove(userTask);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserTaskExists(int id)
    {
        return (_context.Tasks?.Any(e => e.Id == id)).GetValueOrDefault();
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

