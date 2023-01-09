using System.Collections.Immutable;
using System.Diagnostics;
using Volo.Abp.DependencyInjection;

namespace MetricHunter.Core.Processes;

public class ProcessManager : IProcessManager, ISingletonDependency
{
    private readonly List<Process> _processes = new();

    private bool _killAllProcessesRequested;

    public async Task<ProcessResult> RunAsync(ProcessStartInfo processStartInfo)
    {
        if (_killAllProcessesRequested)
            throw new InvalidOperationException(
                "Cannot run a new process after KillAllProcessesAsync() has been called.");

        var process = CreateProcess(processStartInfo);

        string? output = null;
        string? error = null;

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data == null) return;
            error += args.Data + Environment.NewLine;
            processStartInfo.ErrorDataReceived?.Invoke(args.Data);
        };

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data == null) return;
            output += args.Data + Environment.NewLine;
            processStartInfo.OutputDataReceived?.Invoke(args.Data);
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        _processes.Add(process);

        await process.WaitForExitAsync();
        processStartInfo.Exited?.Invoke();

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Output = output,
            Error = error
        };
    }

    public void KillAllProcesses()
    {
        _killAllProcessesRequested = true;
        var processes = _processes.Where(process => !process.HasExited).ToImmutableArray();
        foreach (var process in processes)
        {
            process.Kill(true);
            process.WaitForExit();
            process.Close();
            process.Dispose();
            _processes.Remove(process);
        }
    }

    private static Process CreateProcess(ProcessStartInfo processStartInfo)
    {
        return new Process
        {
            StartInfo =
            {
                Arguments = processStartInfo.Arguments,
                FileName = processStartInfo.Command,
                WorkingDirectory = processStartInfo.WorkingDirectory ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
    }
}