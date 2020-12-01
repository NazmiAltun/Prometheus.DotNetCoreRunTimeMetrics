using System;
using System.Diagnostics.Tracing;

namespace Prometheus.DotNetCoreRunTimeMetrics.Abstraction
{
    internal abstract class StatsCollectorBase : EventListener
    {
        private readonly ICollectorExceptionHandler _errorHandler;

        protected StatsCollectorBase(ICollectorExceptionHandler errorHandler)
        {
            _errorHandler = errorHandler;
            EventSourceCreated += OnEventSourceCreated;
        }

        protected abstract Guid EventSourceGuid { get; }

        protected abstract EventKeywords Keywords { get; }

        protected abstract EventLevel Level { get; }

        protected abstract void ProcessEvent(EventWrittenEventArgs e);

        protected sealed override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            try
            {
                ProcessEvent(eventData);
            }
            catch (Exception e)
            {
                _errorHandler.Handle(e);
            }
        }

        private void OnEventSourceCreated(object sender, EventSourceCreatedEventArgs e)
        {
            var es = e.EventSource;

            if (es?.Guid == EventSourceGuid)
            {
                EnableEvents(es, Level, Keywords);
            }
        }

        public override void Dispose()
        {
            EventSourceCreated -= OnEventSourceCreated;
            base.Dispose();
        }
    }
}