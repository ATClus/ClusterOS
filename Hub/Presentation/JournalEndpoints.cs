using Hub.Domain;
using Hub.Application;
using Microsoft.AspNetCore.Mvc;
namespace Hub.Presentation
{
    public static class JournalEndpoints
    {
        public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/journals", async ([FromServices] IJournalRepository repository) =>
            {
                var journals = await repository.GetAllAsync();
                return Results.Ok(journals);
            });

            endpoints.MapGet("/journals/{id:int}", async ([FromServices] IJournalRepository repository, int id) =>
            {
                var journal = await repository.GetByIdAsync(id);
                return journal is not null ? Results.Ok(journal) : Results.NotFound();
            });

            endpoints.MapGet("/journals/bydate/{date}", async ([FromServices] IJournalRepository repository, string date) =>
            {
                if (DateTime.TryParse(date, out DateTime parsedDate))
                {
                    var journal = await repository.GetByDateAsync(parsedDate);
                    return journal is not null ? Results.Ok(journal) : Results.NotFound();
                }
                return Results.BadRequest(new { error = "Invalid date format" });
            });

            endpoints.MapPost("/journals", async ([FromServices] IJournalRepository repository, [FromBody] Journal journal) =>
            {
                var existingJournal = await repository.GetByDateAsync(journal.Created.Date);
                if (existingJournal != null)
                {
                    existingJournal.UpdateContent(journal.Content);
                    await repository.UpdateAsync(existingJournal);
                    return Results.Ok(existingJournal);
                }

                await repository.AddAsync(journal);
                return Results.Created($"/journals/{journal.Id}", journal);
            });

            endpoints.MapPut("/journals/{id:int}", async ([FromServices] IJournalRepository repository, int id, [FromBody] Journal journal) =>
            {
                var existingJournal = await repository.GetByIdAsync(id);
                if (existingJournal is null)
                    return Results.NotFound();
                try
                {
                    existingJournal.UpdateContent(journal.Content);
                    await repository.UpdateAsync(existingJournal);
                    return Results.Ok(existingJournal);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            endpoints.MapDelete("/journals/{id:int}", async ([FromServices] IJournalRepository repository, int id) =>
            {
                var journal = await repository.GetByIdAsync(id);
                if (journal is null)
                    return Results.NotFound();
                await repository.RemoveAsync(journal);
                return Results.NoContent();
            });

            return endpoints;
        }
    }
}