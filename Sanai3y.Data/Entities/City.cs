namespace Sanai3y.Data.Entities;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<District> Districts { get; set; } = new List<District>();
}