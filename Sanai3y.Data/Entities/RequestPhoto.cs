namespace Sanai3y.Data.Entities;

public class RequestPhoto
{
    public int Id { get; set; }
    public int JobRequestId { get; set; }
    public string Url { get; set; } = string.Empty;

    // Navigation property
    public JobRequest JobRequest { get; set; } = null!;
}