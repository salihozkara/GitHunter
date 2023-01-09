namespace MetricHunter.Core.Processes;

public interface IProcessManager
{
    Task<ProcessResult> RunAsync(ProcessStartInfo processStartInfo);

    void KillAllProcesses();
}