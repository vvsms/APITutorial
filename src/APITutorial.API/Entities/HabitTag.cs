namespace APITutorial.API.Entities;

public class HabitTag
{
    public required string HabitId { get; set; }
    public required string TagId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

}
