using APITutorial.API.Database.Configurations;
using APITutorial.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APITutorial.API.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<HabitTag> HabitTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        // Alternatively, apply configurations individually:
        //modelBuilder.ApplyConfiguration(new HabitConfiguration()); 
    }
}
