using kulunvalvonta.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace kulunvalvonta.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Trafficdata> Trafficdata { get; set; }
    public DbSet<Location> Locations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        var ulidToStringConverter = new ValueConverter<Ulid, string>(
            ulid => ulid.ToString(), // Muutetaan ULID merkkijonoksi tallennettaessa
            str => Ulid.Parse(str)   // Muutetaan ULID takaisin ULID:ksi lukiessa
        );

        modelBuilder.Entity<Trafficdata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasConversion(ulidToStringConverter)
                  .HasMaxLength(26); 
        });
    }

}
