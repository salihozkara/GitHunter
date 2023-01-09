using Octokit;

namespace MetricHunter.Application.Metrics;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LanguageAttribute : Attribute
{
    public LanguageAttribute(params Language[] languages)
    {
        Languages = languages;
    }

    public Language[] Languages { get; }
}