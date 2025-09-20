using System.ComponentModel.DataAnnotations;
using APITutorial.API.Entities;

namespace APITutorial.API.DTOs.Tags;

public sealed record CreateTagDto
{
    [Required]
    [MinLength(3)]
    public required string Name { get; init; }
    [MaxLength(100)]
    public string? Description { get; init; }
}
