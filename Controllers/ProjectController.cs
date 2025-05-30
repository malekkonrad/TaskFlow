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
        var appDbContext = _context.Projects.Include(p => p.Owner).Where(p => p.IsPublic || p.OwnerId == currentUserId)
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
        if (project != null)
        {
            _context.Projects.Remove(project);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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

