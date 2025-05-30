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
public class CommentController : Controller
{
    private readonly AppDbContext _context;

    public CommentController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Comment
    public async Task<IActionResult> Index()
    {
        var commentContext = _context.Comments.Include(c => c.Author);
        return View(await commentContext.ToListAsync());
    }

    // GET: Comment/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Comments == null)
        {
            return NotFound();
        }

        var comment = await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (comment == null)
        {
            return NotFound();
        }

        return View(comment);
    }

    // GET: Comment/Create
    public IActionResult Create()
    {
        ViewData["AuthorId"] = new SelectList(_context.Set<User>(), "Id", "UserName");
        return View();
    }

    // POST: Comment/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Content,TaskItemId")] Comment comment)
    {
        if (ModelState.IsValid)
        {
            Console.WriteLine("Creating comment for TaskItemId: " + comment.TaskItemId);
            comment.AuthorId = GetCurrentUserId();
            comment.CreatedAt = DateTime.Now;



            // DEBUG: Sprawdź wartości przed zapisem
            Console.WriteLine($"=== DEBUG COMMENT CREATE ===");
            Console.WriteLine($"TaskItemId: {comment.TaskItemId}");
            Console.WriteLine($"AuthorId: {comment.AuthorId}");
            Console.WriteLine($"Content: {comment.Content}");
            
            // Sprawdź czy task istnieje
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == comment.TaskItemId);
            Console.WriteLine($"Task exists: {taskExists}");
            
            // Sprawdź czy user istnieje
            var userExists = await _context.Users.AnyAsync(u => u.Id == comment.AuthorId);
            Console.WriteLine($"User exists: {userExists}");
            
            if (!taskExists)
            {
                Console.WriteLine($"ERROR: Task with ID {comment.TaskItemId} does not exist!");
                return RedirectToAction("Index", "UserTask");
            }
            
            if (!userExists)
            {
                Console.WriteLine($"ERROR: User with ID {comment.AuthorId} does not exist!");
                return RedirectToAction("Login", "Auth");
            }









            Console.WriteLine("AuthorId: " + comment.AuthorId);
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "UserTask", new { id = comment.TaskItemId });
        }
        Console.WriteLine("w w w w Creating comment for TaskItemId: " + comment.TaskItemId);
        // ViewData["AuthorId"] = new SelectList(_context.Set<User>(), "Id", "UserName", comment.AuthorId);
        // return View(comment);
        return RedirectToAction("Details", "UserTask", new { id = comment.TaskItemId });
    }

    // GET: Comment/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Comments == null)
        {
            return NotFound();
        }

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        ViewData["AuthorId"] = new SelectList(_context.Set<User>(), "Id", "UserName", comment.AuthorId);
        return View(comment);
    }

    // POST: Comment/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Content,CreatedAt,TaskItemId,AuthorId")] Comment comment)
    {
        if (id != comment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
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
        ViewData["AuthorId"] = new SelectList(_context.Set<User>(), "Id", "UserName", comment.AuthorId);
        return View(comment);
    }

    // GET: Comment/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Comments == null)
        {
            return NotFound();
        }

        var comment = await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (comment == null)
        {
            return NotFound();
        }

        return View(comment);
    }

    // POST: Comment/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Comments == null)
        {
            return Problem("Entity set 'CommentContext.Comment'  is null.");
        }
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CommentExists(int id)
    {
        return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
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

