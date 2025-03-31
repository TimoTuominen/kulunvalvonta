using kulunvalvonta.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace kulunvalvonta.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Trafficdata> Trafficdata { get; set; }
    public DbSet<Location> Locations { get; set; }

}
