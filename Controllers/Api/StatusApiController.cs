using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Controllers.Api;

[ApiController]
[Route("api/statuses")]
public class StatusApiController : ControllerBase
{

    private readonly AppDbContext _context;

    public StatusApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]   // GET: api/statuses/
    public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Statuses == null) return NotFound(); 
        return await _context.Statuses.ToListAsync();
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

    [HttpGet("{id}")] // GET: api/statuses/{id}
    public async Task<ActionResult<Status>> GetStatus(int id)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Projects == null) return NotFound(); 

        var status = await _context.Statuses.FirstOrDefaultAsync(p => p.Id == id);
        if (status == null) return NotFound();
        return status;
    }

    [HttpPost]  // POST: api/statuses/
    public async Task<ActionResult<ProjectDTO>> CreateStatus(Status status)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Statuses == null) return NotFound(); 
        _context.Statuses.Add(status);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStatus), new { id = status.Id }, status);
    }


    [HttpPut("{id}")] // PUT: api/statuses/{id}
    public async Task<IActionResult> UpdateStatus(int id, Status statusDTO)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Statuses == null) return NotFound(); 

        var status = await _context.Projects.FindAsync(id);
        if (status == null) return NotFound();

        status.Name = statusDTO.Name;

        _context.Entry(status).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/statuses/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStatus(int id)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Statuses == null) return NotFound(); 

       
        var status = await _context.Statuses.FindAsync(id);
        if (status == null)return NotFound();
        _context.Statuses.Remove(status);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
    
