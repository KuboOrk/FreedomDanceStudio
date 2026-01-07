namespace FreedomDanceStudio.Models;

public class Service
{
    public int Id { get; set; }
    string Name { get; set; } = null!;
    string? Description { get; set; }
    decimal Price { get; set; }
    int DurationDays { get; set; } // срок действия в днях
}