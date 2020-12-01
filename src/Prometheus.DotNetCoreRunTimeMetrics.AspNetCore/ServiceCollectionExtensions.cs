using Microsoft.Extensions.DependencyInjection;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;

namespace Prometheus.DotNetCoreRunTimeMetrics.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRuntimeStatCollectors(
            this IServiceCollection services,
            RuntimeStatCollectorsConfiguration runtimeStatCollectorsConfiguration = null)
        {
            services.AddSingleton<ContentionStatsCollector>();
            services.AddSingleton<ExceptionStatsCollector>();
            services.AddSingleton<GcStatsCollector>();
            services.AddSingleton<ThreadPoolSchedulingStatsCollector>();
            services.AddSingleton<ThreadPoolStatsCollector>();
            services.AddSingleton<ICollectorExceptionHandler, CollectorExceptionHandler>();
            services.AddSingleton(runtimeStatCollectorsConfiguration ??
                                  RuntimeStatCollectorsConfiguration.Default);

            return services;
        }
    }
}
