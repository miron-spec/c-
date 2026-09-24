namespace MatchApp.Backend.Entities;

public class Report
{
    public Guid Id { get; set; }
    public Guid ReporterProfileId { get; set; }
    public Guid ReportedProfileId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
