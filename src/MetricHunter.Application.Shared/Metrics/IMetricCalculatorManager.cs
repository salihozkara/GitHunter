using Octokit;

namespace MetricHunter.Application.Metrics;

public interface IMetricCalculatorManager
{
    IMetricCalculator FindMetricCalculator(Language language);

    IEnumerable<Language> GetSupportedLanguages();
}