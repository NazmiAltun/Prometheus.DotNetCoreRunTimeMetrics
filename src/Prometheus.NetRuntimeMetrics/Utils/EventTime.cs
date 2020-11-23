using System;

namespace Prometheus.NetRuntimeMetrics.Utils
{
    internal readonly struct EventTime
    {
        public static readonly EventTime Start = new EventTime(EventTimeType.Start);
        public static readonly EventTime FinalWithoutDuration = new EventTime(EventTimeType.FinalWithoutDuration);
        public static readonly EventTime Unrecognized = new EventTime(EventTimeType.Unrecognized);

        private readonly EventTimeType _eventTimeType;

        private EventTime(EventTimeType eventTimeType)
        {
            _eventTimeType = eventTimeType;
            Duration = default;
        }

        public EventTime(TimeSpan duration)
        {
            Duration = duration;
            _eventTimeType = EventTimeType.FinalWithDuration;
        }

        public TimeSpan Duration { get; }

        public bool HasDuration => Duration != default;

        public static bool operator ==(EventTime et1, EventTime et2)
        {
            return et1._eventTimeType == et2._eventTimeType &&
                   et1.Duration == et2.Duration;
        }

        public static bool operator !=(EventTime et1, EventTime et2)
        {
            return et1._eventTimeType != et2._eventTimeType || et1.Duration != et2.Duration;
        }

        public bool Equals(EventTime other)
        {
            return _eventTimeType == other._eventTimeType && Duration == other.Duration;
        }

        public override bool Equals(object obj)
        {
            return obj is EventTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_eventTimeType.GetHashCode(), Duration.GetHashCode());
        }

    }
}