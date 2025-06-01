using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Controllers.Api;

[ApiController]
[Route("api/projects/{projectId}/tasks/{taskId}/comments")]
public class CommentApiController : ControllerBase
{

    private readonly AppDbContext _context;

    public CommentApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]   // GET: api/projects/{projectId}/tasks/{taskId}/comments
    public async Task<ActionResult<IEnumerable<CommentDTO>>> GetProjectTasksComments(int projectId, int taskId)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        var user = await GetAuthorizedUser(Request);
        if (_context.Comments == null) return NotFound();
        if (!await HasProjectAccess(projectId, user))
        {
            return Forbid("You don't have access to this project");
        }

        return await _context.Comments
        .Where(c => c.TaskItemId == taskId && c.Task.ProjectId == projectId)
        .Include(c => c.Author).Select(c => ItemToDTO(c)).ToListAsync();
    }


    
    [HttpGet("{commentId}")] // GET: api/projects/{projectId}/tasks/{taskId}/comments/{commentId}
    public async Task<ActionResult<CommentDTO>> GetProjectTaskComment(int projectId, int taskId, int commentId)
    {
        var user = await GetAuthorizedUser(Request);
        if (user == null) return Unauthorized("Invalid username or token");

        if (!await HasProjectAccess(projectId, user))
        {
            return Forbid("You don't have access to this project");
        }



        var comment = await _context.Comments
        .Include(c => c.Author)
        .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskItemId == taskId && c.Task.ProjectId == projectId);

        if (comment == null) return NotFound();

        return Ok(ItemToDTO(comment));
    }

    [HttpPost]  // GET: api/projects/{projectId}/tasks/{taskId}/comments/
    public async Task<ActionResult<CommentDTO>> CreateTask(int projectId, int taskId, CommentDTO commentDTO)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Comments == null) return NotFound(); 


        var user = await AuthenticateUser(Request);
        var id_owner = user?.Id ?? -1; // Default to 2 if user is null, for testing purposes
        
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return NotFound("Project not found");


        // Tylko właściciel lub członek może dodać task
        if (project.OwnerId != user.Id && !project.Members.Any(m => m.UserId == user.Id))
        {
            return Forbid("Only project owner or members can create tasks");
        }

        // Sprawdź czy Author ma dostęp do projektu 
        User? author = null;
        if (commentDTO.AuthorId.HasValue)
        {
            author = await _context.Users.FindAsync(commentDTO.AuthorId.Value);

            // Sprawdź czy author ma dostęp do projektu - może być nullem
            if (author != null)
            {
                if (project.OwnerId != author.Id && !project.Members.Any(m => m.UserId == author.Id))
                {
                    return BadRequest("Author must be project owner or member");
                }
            }
            
        }


        var comment = new Comment
        {
            Content = commentDTO.Content,
            TaskItemId = taskId,
            AuthorId = author?.Id, // Może być null jeśli nie podano AuthorId
            CreatedAt = DateTime.UtcNow
        };


        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Pobierz task z relacjami dla zwrócenia
        // var createdTask = await _context.Tasks
        //     .Include(t => t.Assignee)
        //     .Include(t => t.Status)
        //     .FirstAsync(t => t.Id == comment.Id);
        return CreatedAtAction(nameof(GetProjectTaskComment), new { projectId = projectId, taskId = taskId, commentId = comment.Id }, ItemToDTO(comment));
    }


    [HttpPut("{commentId}")] // PUT: api/projects/{projectId}/tasks/{taskId}/comments/{commentId}
    public async Task<IActionResult> UpdateComment(int projectId, int taskId, int commentId, CommentDTO commentDTO)
    {
        var user = await GetAuthorizedUser(Request);
        if (user == null) return Unauthorized("Invalid username or token");

        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return NotFound("Project not found");

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task == null) return NotFound("Task not found");

        // Tylko właściciel projektu, członek lub assignee może modyfikować task
        if (commentDTO.AuthorId.HasValue && commentDTO.AuthorId != user.Id)
        {
            // return Forbid("Only author of the comment can update it");

            var author = await _context.Users.FindAsync(commentDTO.AuthorId.Value);

            // Sprawdź czy author ma dostęp do projektu - może być nullem
            if (author != null)
            {
                if (project.OwnerId != author.Id && !project.Members.Any(m => m.UserId == author.Id))
                {
                    return BadRequest("Author must be project owner or member");
                }
            }
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskItemId == taskId && c.Task.ProjectId == projectId);


        if (comment == null) return NotFound("Comment not found");

        comment.Content = commentDTO.Content;
        comment.AuthorId = commentDTO.AuthorId; // Może być null jeśli nie podano AuthorId


        _context.Entry(comment).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    
    [HttpDelete("{commentId}")] // DELETE: api/projects/{projectId}/tasks/{taskId}/comments/{commentId}
    public async Task<IActionResult> DeleteComment(int projectId, int taskId, int commentId)
    {
        var user = await GetAuthorizedUser(Request);
        if (user == null) return Unauthorized("Invalid username or token");

        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return NotFound("Project not found");

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task == null) return NotFound("Task not found");

        // Tylko właściciel projektu lub assignee może usunąć task
        if (project.OwnerId != user.Id && task.AssigneeId != user.Id)
        {
            return Forbid("Only project owner or task assignee can delete this task");
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskItemId == taskId && c.Task.ProjectId == projectId);

        if (comment == null) return NotFound("Comment not found");
        // Sprawdź czy autor ma dostęp do projektu - może być nullem
        if (comment.AuthorId.HasValue)
        {
            var author = await _context.Users.FindAsync(comment.AuthorId.Value);
            if (author != null && project.OwnerId != author.Id && !project.Members.Any(m => m.UserId == author.Id))
            {
                return BadRequest("Author must be project owner or member");
            }
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }




    // private




    private static CommentDTO ItemToDTO(Comment comment) => new CommentDTO
    {
        Id = comment.Id,
        Content = comment.Content,
        TaskItemId = comment.TaskItemId,
        AuthorId = comment.Author?.Id
    };



    private async Task<User?> GetAuthorizedUser(HttpRequest request)
    {
        var username = request.Headers["username"].FirstOrDefault();
        var token = request.Headers["token"].FirstOrDefault();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
        {
            return null;
        }

        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username && u.ApiToken == token);
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

    private async Task<bool> HasProjectAccess(int projectId, User user)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return false;

        // Użytkownik ma dostęp jeśli jest właścicielem, członkiem lub projekt jest publiczny
        return project.OwnerId == user.Id ||
               project.Members.Any(m => m.UserId == user.Id);
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
}
    
