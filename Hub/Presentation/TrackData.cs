using Hub.Domain;
using Hub.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hub.Presentation
{
    public static class TrackData
    {
        public static IEndpointRouteBuilder MapTrackDataEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/track-data", async ([FromServices] ITrackDataRepository repository) =>
            {
                var trackData = await  repository.GetTrackDataAsync();
                return Results.Ok(trackData);
            });

            return endpoints;
        }
    }
}
