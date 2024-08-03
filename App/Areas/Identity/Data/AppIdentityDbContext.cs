using App.Areas.Locations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<Employee>
{
    public DbSet<Location> Locations { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Ignore<AggreggateRoot>();
        builder.Ignore<DomainEvent>();
        builder.Entity<Location>(location =>
        {
            location.HasKey(l => l.Id);
            location
                .Property(l => l.BuildingNumber)
                .IsRequired();
            location
                .Property(l => l.Floor)
                .IsRequired();
        });
    }
}
