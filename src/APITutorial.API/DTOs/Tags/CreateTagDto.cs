using System.ComponentModel.DataAnnotations;
using APITutorial.API.Entities;

namespace APITutorial.API.DTOs.Tags;

public sealed record CreateTagDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
