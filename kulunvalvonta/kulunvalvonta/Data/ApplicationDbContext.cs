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

        // Create a value converter for Ulid to string
        var ulidToStringConverter = new ValueConverter<Ulid, string>(
            ulid => ulid.ToString(), // Convert Ulid to string for storage
            str => Ulid.Parse(str)   // Convert stored string back to Ulid
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
