using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProjectApiController : ControllerBase
{

    private readonly AppDbContext _context;

    public ProjectApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        return await _context.Projects.ToListAsync();
    }

    private bool IsAuthorized(HttpRequest request)
    {

        var role = HttpContext.Session.GetString("Role");
        if (!string.IsNullOrEmpty(role))
        {
            return true;
        }

        var username = request.Headers["username"].FirstOrDefault();
        var token = request.Headers["token"].FirstOrDefault();

        if (username == null || token == null) return false;

        var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.ApiToken == token);
        return user != null;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        
        var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();
        return project;
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(string name, string description)
    {
        if (!IsAuthorized(Request)) return Unauthorized();

        var id_owner = int.Parse(HttpContext.Session.GetString("Id") ?? "2");
        Console.WriteLine($"Creating project for owner ID: {id_owner}");
        var owner = await _context.Users.FindAsync(id_owner);
        Console.WriteLine($"Owner found: {owner?.UserName}");

        var project = new Project
        {
            Name = name,
            OwnerId = id_owner,
            // Owner = owner, // Set the owner of the project
            Description = description, // Add this line
            Members = new List<ProjectMember>(), // Initialize Members collection
            // Tasks = new List<Task>(), // Initialize Tasks collection
            
            IsPublic = false // Default value, can be changed later
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }
}
