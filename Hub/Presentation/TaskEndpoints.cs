using Hub.Application;
using Hub.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Hub.Presentation
{
    public static class TaskEndpoints
    {
        public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/tasks", async ([FromServices] ITaskItemRepository repository) =>
            {
                var tasks = await repository.GetAllAsync();
                return Results.Ok(tasks);
            });

            endpoints.MapGet("/tasks/{id:int}", async ([FromServices] ITaskItemRepository repository, int id) =>
            {
                var task = await repository.GetByIdAsync(id);
                return task is not null ? Results.Ok(task) : Results.NotFound();
            });

            endpoints.MapPost("/tasks", async ([FromServices] ITaskItemRepository repository, [FromBody] TaskItem task) =>
            {
                await repository.AddAsync(task);
                return Results.Created($"/tasks/{task.Id}", task);
            });

            endpoints.MapPut("/tasks/{id:int}", async ([FromServices] ITaskItemRepository repository, int id, [FromBody] TaskItem task) =>
            {
                var existingTask = await repository.GetByIdAsync(id);
                if (existingTask is null)
                    return Results.NotFound();

                existingTask.Update(task.Title, task.Description, task.Priority);
                await repository.UpdateAsync(existingTask);
                return Results.Ok(existingTask);
            });

            endpoints.MapDelete("/tasks/{id:int}", async ([FromServices] ITaskItemRepository repository, int id) =>
            {
                var existingTask = await repository.GetByIdAsync(id);
                if (existingTask is null)
                    return Results.NotFound();

                await repository.RemoveAsync(existingTask);
                return Results.NoContent();
            });

            return endpoints;
        }
    }
}
