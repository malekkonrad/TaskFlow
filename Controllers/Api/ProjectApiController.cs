using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;

namespace TaskFlow.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProjectApiController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        // API logic
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        // API logic
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        // API logic
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }
}
