using APITutorial.API.Entities;

namespace APITutorial.API.DTOs.Tags;

public static class TagMappings
{

    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc,
        };
    }


    public static Tag ToEntity(this CreateTagDto dto)
    {
        Tag Tag = new()
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow,
        };

        return Tag;
    }

    public static void UpdateFromDto(this Tag Tag, UpdateTagDto dto)
    {
        Tag.Name = dto.Name;
        Tag.Description = dto.Description;
        Tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}