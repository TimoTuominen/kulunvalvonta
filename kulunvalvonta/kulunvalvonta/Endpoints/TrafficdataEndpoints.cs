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
        // Nouda kaikki liikenne tiedot
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
                                   LocationId = t.LocationId,
                                   LocationName = t.Location.LocationName 
                               })
                               .ToListAsync();
            return Results.Ok(data);
        });


        // Lisää uusi liikenne tieto

        app.MapPost("/trafficdata", async (Kulunvalvonta.Shared.TrafficdataDto newEntry, ApplicationDbContext db) =>
        {
          
            var trafficdata = new Trafficdata
            {
             
                Id = Ulid.NewUlid(),
                RegNumber = newEntry.RegNumber,
                DriverName = newEntry.DriverName,
                Company = newEntry.Company,
                PhoneNumber = newEntry.PhoneNumber,
                Date = newEntry.Date,
                EntryTime = newEntry.EntryTime,
                ExitTime = newEntry.ExitTime,
                Reason = newEntry.Reason,
                ExpandedReason = newEntry.ExpandedReason,
                LocationId = newEntry.LocationId
            };

            db.Trafficdata.Add(trafficdata);
            await db.SaveChangesAsync();
          
            return Results.Created($"/trafficdata/{trafficdata.Id}", trafficdata);
        });


        // Hae liikenne data id:n perusteella
        app.MapGet("/trafficdata/{id}", async (string id, ApplicationDbContext db) =>
        {
            if (!Ulid.TryParse(id, out var ulid))
            {
                return Results.BadRequest("Invalid Ulid format.");
            }

            var data = await db.Trafficdata.FindAsync(ulid);
            return data is not null ? Results.Ok(data) : Results.NotFound();
        });


        // Hae sijainnit
        app.MapGet("/locations", async (ApplicationDbContext db) =>
        {
            var locations = await db.Locations.ToListAsync();
            return Results.Ok(locations);
        });

        // Luo uusi sijainti
        app.MapPost("/locations", async (Location newLocation, ApplicationDbContext db) =>
        {
            db.Locations.Add(newLocation);
            await db.SaveChangesAsync();
            return Results.Created($"/locations/{newLocation.LocationId}", newLocation);
        });


        return app;
    }
}
