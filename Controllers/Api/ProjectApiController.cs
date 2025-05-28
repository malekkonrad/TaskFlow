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

    [HttpGet]   // GET: api/ProjectApi/
    public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Projects == null) return NotFound(); 
        return await _context.Projects.Select(x => ItemToDTO(x)).ToListAsync();
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

    [HttpGet("{id}")] // GET: api/ProjectApi/{id}
    public async Task<ActionResult<ProjectDTO>> GetProject(int id)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Projects == null) return NotFound(); 

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return NotFound();
        return ItemToDTO(project);
    }

    [HttpPost]  // POST: api/ProjectApi/
    public async Task<ActionResult<ProjectDTO>> CreateProject(ProjectDTO projectDTO)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Projects == null) return NotFound(); 

        var id_owner = int.Parse(HttpContext.Session.GetString("Id") ?? "2");
        Console.WriteLine($"Creating project for owner ID: {id_owner}");
        var owner = await _context.Users.FindAsync(id_owner);
        Console.WriteLine($"Owner found: {owner?.UserName}");

        var project = new Project
        {
            Name = projectDTO.Name,
            OwnerId = id_owner,
            // Owner = owner, // Set the owner of the project
            Description = projectDTO.Description, // Add this line
            Members = new List<ProjectMember>(), // Initialize Members collection
                                                 // Tasks = new List<Task>(), // Initialize Tasks collection
            IsPublic = projectDTO.IsPublic // Default value, can be changed later
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, ItemToDTO(project));
    }


    [HttpPut("{id}")] // PUT: api/ProjectApi/{id}
    public async Task<IActionResult> UpdateProject(int id, ProjectDTO projectDTO)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Projects == null) return NotFound(); 

        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();

        project.Name = projectDTO.Name;
        project.Description = projectDTO.Description;
        project.IsPublic = projectDTO.IsPublic;

        _context.Entry(project).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ProjectDTO ItemToDTO(Project project) => new ProjectDTO
    {
        Name = project.Name,
        Description = project.Description,
        IsPublic = project.IsPublic
    };
}
    
