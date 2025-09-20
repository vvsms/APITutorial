using APITutorial.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APITutorial.API.Database.Configurations;

public class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(500);

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.OwnsOne(x => x.Frequency);

        builder.OwnsOne(x => x.Target, targetBuilder =>
        {
            targetBuilder.Property(x => x.Unit).HasMaxLength(100);
        });

        builder.OwnsOne(x => x.Milestone);

        builder.HasMany(h => h.Tags)
            .WithMany()
            .UsingEntity<HabitTag>();
    }
}
