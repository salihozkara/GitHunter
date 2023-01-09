namespace MetricHunter.Core.Processes;

public class ProcessResult
{
    public int ExitCode { get; init; }
    public string? Output { get; init; }
    public string? Error { get; init; }
}