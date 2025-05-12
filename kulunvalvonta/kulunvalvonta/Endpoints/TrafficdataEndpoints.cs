using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using kulunvalvonta.Data.Models;
using kulunvalvonta.Data;
using Kulunvalvonta.Shared;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

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
                [FromQuery] DateTime? start,      
                [FromQuery] DateTime? end,        
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

        // Tietojen exporttaaminen Exceliin

        app.MapGet("/api/export/excel", async (
            HttpContext context,
            [FromQuery] string? regNumber,
            [FromQuery] string? company,
            [FromQuery] string? driverName,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] int? locationId,
            ApplicationDbContext db) =>
        {
            var query = db.Trafficdata.Include(x => x.Location).AsQueryable();

            if (!string.IsNullOrWhiteSpace(regNumber))
                query = query.Where(x => x.RegNumber == regNumber);

            if (!string.IsNullOrWhiteSpace(company))
                query = query.Where(x => x.Company == company);

            if (!string.IsNullOrWhiteSpace(driverName))
                query = query.Where(x => x.DriverName == driverName);

            // Muuta hakuehdot DateOnly ja TimeOnly muotoon
            if (start.HasValue)
            {
                var startDate = DateOnly.FromDateTime(start.Value);
                query = query.Where(x => x.Date >= startDate);
            }

            if (end.HasValue)
            {
                var endDate = DateOnly.FromDateTime(end.Value);
                query = query.Where(x => x.Date <= endDate);
            }

            if (locationId.HasValue && locationId.Value != 0)
                query = query.Where(x => x.LocationId == locationId.Value);

            var results = await query.ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Results");

            ws.Cell(1, 1).Value = "Rekisterinumero";
            ws.Cell(1, 2).Value = "Kuljettaja";
            ws.Cell(1, 3).Value = "Yritys";
            ws.Cell(1, 4).Value = "Päivämäärä";
            ws.Cell(1, 5).Value = "Sisään";
            ws.Cell(1, 6).Value = "Ulos";
            ws.Cell(1, 7).Value = "Syy";
            ws.Cell(1, 8).Value = "Lisätiedot";
            ws.Cell(1, 9).Value = "Paikkakunta";

            for (int i = 0; i < results.Count; i++)
            {
                var row = i + 2;
                var item = results[i];
                ws.Cell(row, 1).Value = item.RegNumber;
                ws.Cell(row, 2).Value = item.DriverName;
                ws.Cell(row, 3).Value = item.Company;
                ws.Cell(row, 4).Value = item.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 5).Value = item.EntryTime?.ToString(@"HH\:mm");
                ws.Cell(row, 6).Value = item.ExitTime?.ToString(@"HH\:mm");
                ws.Cell(row, 7).Value = item.Reason?.ToString() ?? "";
                ws.Cell(row, 8).Value = item.ExpandedReason;
                ws.Cell(row, 9).Value = item.Location?.LocationName;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.Headers.Append("Content-Disposition", "attachment; filename=results.xlsx");
            await context.Response.BodyWriter.WriteAsync(ms.ToArray());
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
