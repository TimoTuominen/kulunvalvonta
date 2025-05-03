using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using kulunvalvonta.Data.Models;
using kulunvalvonta.Data;
using Kulunvalvonta.Shared;
using Microsoft.AspNetCore.Mvc;

public static class TrafficdataEndpoints
{
    public static IEndpointRouteBuilder MapTrafficdataEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapGet("/trafficdata", async (ApplicationDbContext db) =>
        {
            var open = await db.Trafficdata
                .Where(t => t.ExitTime == null)
                .Select(t => new TrafficdataDto
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

            return Results.Ok(open);
        });

        // Hake liikenne tiedot hakuehdoilla
        app.MapGet("/trafficdata/search", async (
                [FromQuery] string? regNumber,
                [FromQuery] string? driverName,
                [FromQuery] string? company,
                [FromQuery] DateTime? start,      // combined ISO date+time
                [FromQuery] DateTime? end,        // combined ISO date+time
                [FromQuery] int? locationId,
                [FromQuery] int skip,
                [FromQuery] int take,
                ApplicationDbContext db) =>
        {
            IQueryable<Trafficdata> q = db.Trafficdata;

            if (!string.IsNullOrWhiteSpace(regNumber))
                q = q.Where(t => t.RegNumber.Contains(regNumber));

            if (!string.IsNullOrWhiteSpace(driverName))
                q = q.Where(t => t.DriverName!.Contains(driverName));

            if (!string.IsNullOrWhiteSpace(company))
                q = q.Where(t => t.Company!.Contains(company));

            // BETWEEN filter on Date + EntryTime
            if (start.HasValue || end.HasValue)
            {
                var startDate = start.HasValue
                    ? DateOnly.FromDateTime(start.Value)
                    : DateOnly.MinValue;
                var startTime = start.HasValue
                    ? TimeOnly.FromDateTime(start.Value)
                    : TimeOnly.MinValue;

                var endDate = end.HasValue
                    ? DateOnly.FromDateTime(end.Value)
                    : DateOnly.MaxValue;
                var endTime = end.HasValue
                    ? TimeOnly.FromDateTime(end.Value)
                    : TimeOnly.MaxValue;

                q = q.Where(t =>
                    (
                        t.Date > startDate
                        || (t.Date == startDate && t.EntryTime! >= startTime)
                    )
                    &&
                    (
                        t.Date < endDate
                        || (t.Date == endDate && t.EntryTime! <= endTime)
                    )
                );
            }

            if (locationId.HasValue && locationId.Value != 0)
                q = q.Where(t => t.LocationId == locationId.Value);

            var total = await q.CountAsync();

            var page = await q
                .OrderByDescending(t => t.Date)
                .ThenBy(t => t.EntryTime)
                .Skip(skip)
                .Take(take)
                .Select(t => new TrafficdataDto
                {
                    Id = t.Id.ToString(),
                    RegNumber = t.RegNumber,
                    DriverName = t.DriverName,
                    Company = t.Company,
                    Date = t.Date,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    Reason = t.Reason,
                    ExpandedReason = t.ExpandedReason,
                    LocationId = t.LocationId,
                    LocationName = t.Location.LocationName
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Items = page,
                TotalCount = total
            });
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

        // Poista liikenne tieto id:n perusteella

        app.MapDelete("/trafficdata/{id}", async (string id, ApplicationDbContext db) =>
        {
            if (!Ulid.TryParse(id, out var ulid))
                return Results.BadRequest("Invalid ULID format.");

            var entity = await db.Trafficdata.FindAsync(ulid);
            if (entity is null)
                return Results.NotFound();

            db.Trafficdata.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });


        // Merkkaa liikenne tieto ulos
        app.MapPost("/trafficdata/{id}/exit", async (string id, ApplicationDbContext db) =>
        {
            if (!Ulid.TryParse(id, out var ulid))
                return Results.BadRequest("Invalid ULID format.");

            var entity = await db.Trafficdata.FindAsync(ulid);
            if (entity is null)
                return Results.NotFound();

            // stamp the current server time
            entity.ExitTime = TimeOnly.FromDateTime(DateTime.Now);

            await db.SaveChangesAsync();
            return Results.NoContent();
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
