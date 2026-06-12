namespace Application.DTOs.Jobs;

public class ProcessPendingOrdersOutDto
{
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
