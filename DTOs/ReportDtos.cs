namespace MatchApp.Backend.DTOs;
public record CreateReportRequest(Guid ReportedProfileId, string Reason);
