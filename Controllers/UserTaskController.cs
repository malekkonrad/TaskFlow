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
        var appDbContext = _context.Tasks.Include(u => u.Assignee).Include(u => u.Project).Include(u => u.Status);
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

    // GET: UserTask/Create
    public IActionResult Create(int? projectId)
    {
        ViewData["AssigneeId"] = new SelectList(_context.Users, "Id", "UserName");
        if (projectId.HasValue)
        {
            // Jeśli mamy projectId, ustaw go jako wybrany i pokaż tylko ten projekt
            ViewData["ProjectId"] = new SelectList(_context.Projects.Where(p => p.Id == projectId), "Id", "Name", projectId);
            ViewData["SelectedProjectId"] = projectId;
        }
        else
        {
            // Jeśli nie ma projectId, pokaż wszystkie projekty
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
        }
        
        ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Name");
        return View();
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
}

