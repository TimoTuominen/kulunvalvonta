using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using kulunvalvonta.Data.Models;
using kulunvalvonta.Data;
using Kulunvalvonta.Shared;

public static class TrafficdataEndpoints
{
    public static IEndpointRouteBuilder MapTrafficdataEndpoints(this IEndpointRouteBuilder app)
    {
        // GET ALL DATA
        app.MapGet("/trafficdata", async (ApplicationDbContext db) =>
        {
            var data = await db.Trafficdata
                               .Select(t => new Kulunvalvonta.Shared.TrafficdataDto
                               {
                                   Id = t.Id.ToString(), 
                                   RegNumber = t.RegNumber,
                                   DriverName = t.DriverName,
                                   Company = t.Company,
                                   PhoneNumber = t.PhoneNumber,
                                   Date = t.Date,
                                   EntryTime = t.EntryTime,
                                   ExitTime = t.ExitTime,
                                   Reason = t.Reason,
                                   ExpandedReason = t.ExpandedReason,
                                   LocationId = t.LocationId
                               })
                               .ToListAsync();
            return Results.Ok(data);
        });

        // GET DATA BY ID
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
