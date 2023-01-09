using MetricHunter.Application.Results;
using Octokit;
using Volo.Abp.DependencyInjection;

namespace MetricHunter.Application.Metrics;

public interface IMetricCalculator : ISingletonDependency
{
    Task<IResult> CalculateMetricsAsync(Repository repository, CancellationToken token = default);

    Task<IResult?[]> CalculateMetricsByLocalResultsAsync(List<Repository> repositories,
        CancellationToken token = default);
}