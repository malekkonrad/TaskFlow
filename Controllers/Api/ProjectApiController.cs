using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Controllers.Api;

[ApiController]
[Route("api/projects")]
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

    private async Task<User?> AuthenticateUser(HttpRequest request)
    {
        var username = request.Headers["username"].FirstOrDefault();
        var token = request.Headers["token"].FirstOrDefault();
        Console.WriteLine($"Authenticating user: {username} with token: {token}");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
        {
            return null;
        }

        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username && u.ApiToken == token);
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


        var user = await AuthenticateUser(Request);
        var id_owner = user?.Id ?? -1; 

        var project = new Project
        {
            
            Name = projectDTO.Name,
            OwnerId = id_owner,
            Owner = user, 
            Description = projectDTO.Description, 
            Members = new List<ProjectMember>(),                   
            IsPublic = projectDTO.IsPublic 
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

    // DELETE: api/Project/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Projects == null) return NotFound(); 

        var user = await AuthenticateUser(Request);
        if (user == null)
        {
            return Unauthorized("Invalid username or API token");
        }
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Only owner can delete project
        if (project.OwnerId != user.Id)
        {
            return Forbid("Only project owner can delete the project");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ProjectDTO ItemToDTO(Project project) => new ProjectDTO
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        IsPublic = project.IsPublic
    };
}
    
