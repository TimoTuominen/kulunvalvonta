using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using kulunvalvonta.Data.Models;
using kulunvalvonta.Data;

public static class TrafficdataEndpoints
{
    public static IEndpointRouteBuilder MapTrafficdataEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint to get all Trafficdata records
        app.MapGet("/trafficdata", async (ApplicationDbContext db) =>
        {
            var data = await db.Trafficdata.ToListAsync();
            return Results.Ok(data);
        });

        // Endpoint to get a specific Trafficdata record by Id
        app.MapGet("/trafficdata/{id}", async (string id, ApplicationDbContext db) =>
        {
            if (!Ulid.TryParse(id, out var ulid))
            {
                return Results.BadRequest("Invalid Ulid format.");
            }

            var data = await db.Trafficdata.FindAsync(ulid);
            return data is not null ? Results.Ok(data) : Results.NotFound();
        });

        return app;
    }
}
