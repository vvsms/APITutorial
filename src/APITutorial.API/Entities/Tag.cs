﻿namespace APITutorial.API.Entities;

public class Tag
{
    public required string Id { get; set; }
    public string Name { get; set; }=string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
