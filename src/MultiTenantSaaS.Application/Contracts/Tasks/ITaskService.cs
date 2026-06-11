namespace MultiTenantSaaS.Application.Contracts.Tasks;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> ListAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default);

    Task<TaskDto> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<TaskDto> UpdateStatusAsync(
        Guid taskId,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default);
}
