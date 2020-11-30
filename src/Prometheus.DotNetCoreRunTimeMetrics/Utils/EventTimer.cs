using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Caching.Memory;

namespace Prometheus.DotNetCoreRunTimeMetrics.Utils
{
    internal class EventTimer
    {
        private readonly TimeSpan DefaultCacheDuration = TimeSpan.FromSeconds(60);

        private readonly IMemoryCache _memoryCache;
        private readonly int _startEventId;
        private readonly int _endEventId;
        private readonly Func<EventWrittenEventArgs, long> _extractDataIdFunc;
        private readonly string _prefix;

        public EventTimer(
            IMemoryCache memoryCache,
            int startEventId,
            int endEventId,
            Func<EventWrittenEventArgs, long> extractDataIdFunc)
        : this(memoryCache, startEventId, endEventId, extractDataIdFunc, string.Empty)
        {
        }

        public EventTimer(
            IMemoryCache memoryCache,
            int startEventId,
            int endEventId,
            Func<EventWrittenEventArgs, long> extractDataIdFunc,
            string prefix)
        {
            _startEventId = startEventId;
            _endEventId = endEventId;
            _memoryCache = memoryCache;
            _extractDataIdFunc = extractDataIdFunc;
            _prefix = prefix;
        }

        public EventTime GetEventTime(EventWrittenEventArgs e)
        {
            var key = $"{_prefix}_{_extractDataIdFunc(e)}";

            if (e.EventId == _startEventId)
            {
                _memoryCache.Set(key, e.TimeStamp, DefaultCacheDuration);
                return EventTime.Start;
            }

            if (e.EventId == _endEventId)
            {
                if (_memoryCache.TryGetValue(key, out DateTime timeStamp))
                {
                    var eventTime = new EventTime(e.TimeStamp - timeStamp);
                    _memoryCache.Remove(key);
                    return eventTime;
                }

                return EventTime.FinalWithoutDuration;
            }

            return EventTime.Unrecognized;
        }
    }
}