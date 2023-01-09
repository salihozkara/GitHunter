using MetricHunter.Application.Results;
using Microsoft.Extensions.Logging;
using Octokit;

namespace MetricHunter.Application.Metrics;

public class NullMetricCalculator : IMetricCalculator
{
    private readonly ILogger<NullMetricCalculator> _logger;

    public NullMetricCalculator(ILogger<NullMetricCalculator> logger)
    {
        _logger = logger;
    }

    public Task<IResult> CalculateMetricsAsync(Repository repository, CancellationToken token = default)
    {
        _logger.LogInformation("No language statistics available for {Repository}", repository.Name);
        return Task.FromResult<IResult>(new NullResult());
    }

    public Task<IResult?[]> CalculateMetricsByLocalResultsAsync(List<Repository> repositories,
        CancellationToken token = default)
    {
        _logger.LogInformation("No language statistics available for {RepositoryCount} repositories",
            repositories.Count);
        return Task.FromResult(Array.Empty<IResult?>());
    }
}