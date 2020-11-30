using System;

namespace Prometheus.DotNetCoreRunTimeMetrics.Abstraction
{
    internal abstract class RuntimeStatsCollectorBase : StatsCollectorBase
    {
        protected RuntimeStatsCollectorBase(Action<Exception> errorHandler) 
            : base(errorHandler)
        {
        }

        protected sealed override Guid EventSourceGuid { get; } = Guid.Parse("5e5bb766-bbfc-5662-0548-1d44fad9bb56");
    }
}