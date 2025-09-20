﻿using APITutorial.API.Entities;

namespace APITutorial.API.DTOs.Tags;

public sealed record TagsCollectionDto
{
    public required List<TagDto> Data { get; init; }
}
public sealed record TagDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}