using APITutorial.API.DTOs.Common;

namespace APITutorial.API.DTOs.Tags;

public sealed record TagsCollectionDto : ICollectionResponse<TagDto>
{
    public required List<TagDto> Items { get; init; }
}
