using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using Volo.Abp.DependencyInjection;

namespace MetricHunter.Application.Metrics;

public class MetricCalculatorManager : IMetricCalculatorManager, ISingletonDependency
{
    private readonly ILogger<MetricCalculatorManager> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MetricCalculatorManager(IServiceProvider serviceProvider, ILogger<MetricCalculatorManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IMetricCalculator FindMetricCalculator(Language language)
    {
        var languageStatistics = _serviceProvider.GetRequiredService<IEnumerable<IMetricCalculator>>()
            .SingleOrDefault(t =>
                t.GetType().GetCustomAttribute<LanguageAttribute>()?.Languages.Contains(language) ?? false);

        if (languageStatistics == null)
        {
            _logger.LogWarning($"No language statistics found for language {language}");
            return _serviceProvider.GetRequiredService<NullMetricCalculator>();
        }

        return languageStatistics;
    }

    public IEnumerable<Language> GetSupportedLanguages()
    {
        return _serviceProvider.GetRequiredService<IEnumerable<IMetricCalculator>>()
            .SelectMany(t => t.GetType().GetCustomAttribute<LanguageAttribute>()?.Languages ?? Array.Empty<Language>())
            .Distinct()
            .ToArray();
    }
}