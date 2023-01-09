using System.Reflection;
using Volo.Abp.Modularity;

namespace MetricHunter.Core.Modules;

public class MetricHunterModule : AbpModule
{
    public static bool IsMetricHunterModule(Type type)
    {
        var typeInfo = type.GetTypeInfo();

        return
            typeInfo is { IsClass: true, IsAbstract: false, IsGenericType: false } &&
            typeof(MetricHunterModule).GetTypeInfo().IsAssignableFrom(type);
    }
}