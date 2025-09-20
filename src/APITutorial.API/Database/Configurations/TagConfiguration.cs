using APITutorial.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APITutorial.API.Database.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(500);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(t => new { t.Name }).IsUnique();
    }
}
