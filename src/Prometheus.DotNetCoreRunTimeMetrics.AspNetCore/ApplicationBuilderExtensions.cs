using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.DotNetCoreRunTimeMetrics.Collectors;

namespace Prometheus.DotNetCoreRunTimeMetrics.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder StartCollectingRuntimeStats(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<ContentionStatsCollector>();
            app.ApplicationServices.GetService<ExceptionStatsCollector>();
            app.ApplicationServices.GetService<GcStatsCollector>();
            app.ApplicationServices.GetService<ThreadPoolStatsCollector>();
            app.ApplicationServices.GetService<ThreadPoolSchedulingStatsCollector>();

            return app;
        }
    }
}