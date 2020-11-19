using System;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Abstraction
{
    //TODO:Do we really need this interface?
    internal interface IEventSourceCollector
    {
        Guid EventSourceGuid { get; }

        EventKeywords Keywords { get; }

        EventLevel Level { get; }

        void ProcessEvent(EventWrittenEventArgs e);
    }
}
