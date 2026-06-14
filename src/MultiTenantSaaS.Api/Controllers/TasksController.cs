using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiTenantSaaS.Application.Contracts.Tasks;
using MultiTenantSaaS.Shared.Constants;

namespace MultiTenantSaaS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.TasksView)]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List(
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        var tasks = await _taskService.ListAsync(projectId, cancellationToken);
        return Ok(tasks);
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.TasksCreate)]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var task = await _taskService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(List), new { projectId = task.ProjectId }, task);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Task creation failed");
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PermissionPolicies.TasksUpdateStatus)]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var task = await _taskService.UpdateStatusAsync(id, request, cancellationToken);
            return Ok(task);
        }
        catch (InvalidOperationException ex)
        {
            var statusCode = ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Problem(
                detail: ex.Message,
                statusCode: statusCode,
                title: "Task status update failed");
        }
    }
}
