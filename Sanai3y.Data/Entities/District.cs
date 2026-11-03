namespace Sanai3y.Data.Entities;

public class District
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation properties
    public City City { get; set; } = null!;
}