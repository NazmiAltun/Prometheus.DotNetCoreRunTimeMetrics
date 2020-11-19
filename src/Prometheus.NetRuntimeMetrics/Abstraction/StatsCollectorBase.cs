using System;
using System.Diagnostics.Tracing;

namespace Prometheus.NetRuntimeMetrics.Abstraction
{
    internal abstract class StatsCollectorBase : EventListener, IEventSourceCollector
    {
        private readonly Action<Exception> _errorHandler;

        protected StatsCollectorBase(Action<Exception> errorHandler)
        {
            _errorHandler = errorHandler;
            EventSourceCreated += OnEventSourceCreated;
        }

        public abstract Guid EventSourceGuid { get; }

        public abstract EventKeywords Keywords { get; }

        public abstract EventLevel Level { get; }

        public abstract void ProcessEvent(EventWrittenEventArgs e);

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            try
            {
                ProcessEvent(eventData);
            }
            catch (Exception e)
            {
                _errorHandler(e);
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