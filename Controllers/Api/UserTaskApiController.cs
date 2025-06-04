using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

namespace TaskFlow.Controllers.Api;

[ApiController]
[Route("api/projects/{projectId}/tasks")]
public class UserTaskApiController : ControllerBase
{

    private readonly AppDbContext _context;

    public UserTaskApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]   // GET: api/projects/{projectId}/tasks/
    public async Task<ActionResult<IEnumerable<UserTaskDTO>>> GetProjectTasks(int projectId)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        var user = await GetAuthorizedUser(Request);
        if (_context.Projects == null) return NotFound();
        if (!await HasProjectAccess(projectId, user))
        {
            return Forbid("You don't have access to this project");
        }
        return await _context.Tasks
        .Where(t => t.ProjectId == projectId)
        .Include(t => t.Assignee)
        .Include(t => t.Status)
        .Select(x => ItemToDTO(x)).ToListAsync();
    }


    // GET: api/Project/5/tasks/3
    [HttpGet("{taskId}")]
    public async Task<ActionResult<UserTaskDTO>> GetProjectTask(int projectId, int taskId)
    {
        var user = await GetAuthorizedUser(Request);
        if (user == null) return Unauthorized("Invalid username or token");

        if (!await HasProjectAccess(projectId, user))
        {
            return Forbid("You don't have access to this project");
        }

        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task == null) return NotFound();

        return Ok(ItemToDTO(task));
    }

    [HttpPost]  // POST: api/projects/{projectId}/tasks/
    public async Task<ActionResult<UserTaskDTO>> CreateTask(int projectId, UserTaskDTO userTaskDTO)
    {
        if (!IsAuthorized(Request)) return Unauthorized();
        if (_context.Tasks == null) return NotFound(); 


        var user = await AuthenticateUser(Request);
        var id_owner = user?.Id ?? -1; 
        
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return NotFound("Project not found");

        if (project.OwnerId != user.Id && !project.Members.Any(m => m.UserId == user.Id))
        {
            return Forbid("Only project owner or members can create tasks");
        }

        User? assignee = null;
        if (userTaskDTO.AssigneeId.HasValue)
        {
            assignee = await _context.Users.FindAsync(userTaskDTO.AssigneeId.Value);
            if (assignee == null) return BadRequest("Assignee not found");

            if (project.OwnerId != assignee.Id && !project.Members.Any(m => m.UserId == assignee.Id))
            {
                return BadRequest("Assignee must be project owner or member");
            }
        }

        var task = new UserTask
        {
            Title = userTaskDTO.Title,
            Description = userTaskDTO.Description,
            ProjectId = projectId,
            AssigneeId = userTaskDTO.AssigneeId,
            CreatedAt = DateTime.UtcNow,
        };


        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var createdTask = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Status)
            .FirstAsync(t => t.Id == task.Id);

        
        return CreatedAtAction(nameof(GetProjectTask), new { projectId = projectId, taskId = task.Id }, ItemToDTO(createdTask));
    }


    [HttpPut("{taskId}")] // PUT: api/projects/{id_project}/tasks/{task_id}
    public async Task<IActionResult> UpdateTask(int projectId, int taskId, UserTaskDTO taskDTO)
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

        if (project.OwnerId != user.Id && 
            !project.Members.Any(m => m.UserId == user.Id) && 
            task.AssigneeId != user.Id)
        {
            return Forbid("You don't have permission to modify this task");
        }

        if (taskDTO.AssigneeId.HasValue)
        {
            var assignee = await _context.Users.FindAsync(taskDTO.AssigneeId.Value);
            if (assignee == null) return BadRequest("Assignee not found");

            if (project.OwnerId != assignee.Id && !project.Members.Any(m => m.UserId == assignee.Id))
            {
                return BadRequest("Assignee must be project owner or member");
            }
        }

        task.Title = taskDTO.Title;
        task.Description = taskDTO.Description;
        task.AssigneeId = taskDTO.AssigneeId;

        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Project/5/tasks/3
    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int projectId, int taskId)
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

        if (project.OwnerId != user.Id && task.AssigneeId != user.Id)
        {
            return Forbid("Only project owner or task assignee can delete this task");
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }




    // private

    private static UserTaskDTO ItemToDTO(UserTask userTask) => new UserTaskDTO
    {
        Id = userTask.Id,
        Title = userTask.Title,
        Description = userTask.Description,
        ProjectId = userTask.ProjectId,
        AssigneeId = userTask.AssigneeId

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
    
