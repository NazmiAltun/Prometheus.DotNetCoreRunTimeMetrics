using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using AutoFixture.Xunit2;
using Fasterflect;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Prometheus.NetRuntimeMetrics.Utils;
using Xunit;

namespace Prometheus.NetRuntimeMetrics.Tests.Utils
{
    public class EventTimerTests
    {
        [Theory]
        [AutoData]
        public void Given_ItIsAnEndEvent_And_SameEventIdProvided_Then_EventTimeShouldBeFinalWithDuration(
            int endEventId,
            int startEventId,
            int eventId)
        {
            var sut = CreateTimer(startEventId, endEventId);
            var dateTime = DateTime.UtcNow;
            var startEventArgs = CreateEventWrittenEventArgs(eventId, dateTime.AddSeconds(-10));
            startEventArgs.SetPropertyValue("EventId", startEventId);
            sut.GetEventTime(startEventArgs);
            var endEventArgs = CreateEventWrittenEventArgs(eventId, dateTime.AddSeconds(10));
            endEventArgs.SetPropertyValue("EventId", endEventId);
            var eventTime = sut.GetEventTime(endEventArgs);
            eventTime.Should().Be(new EventTime(TimeSpan.FromSeconds(20)));
        }

        [Theory]
        [AutoData]
        public void Given_ItIsAnEndEvent_And_PayloadIsDifferent_Then_EventTimeShouldBeFinalWithoutDuration(
            int endEventId,
            int startEventId)
        {
            var sut = CreateTimer(startEventId, endEventId);
            var eventArgs = CreateEventWrittenEventArgs(default, default);
            eventArgs.SetPropertyValue("EventId", endEventId);
            sut.GetEventTime(eventArgs).Should().Be(EventTime.FinalWithoutDuration);
        }

        [Theory]
        [AutoData]
        public void Given_ItIsAStartEvent_Then_EventTimeShouldBeStart(
            int endEventId,
            int startEventId)
        {
            var sut = CreateTimer(startEventId, endEventId);
            var eventArgs = CreateEventWrittenEventArgs(default, default);
            eventArgs.SetPropertyValue("EventId", startEventId);
            sut.GetEventTime(eventArgs).Should().Be(EventTime.Start);
        }

        [Theory]
        [AutoData]
        public void Given_EventIdDoNotMatchStartOrEndEventId_Then_EventTimeShouldBeUnrecognized(
            int endEventId,
            int startEventId)
        {
            var sut = CreateTimer(startEventId, endEventId);
            var eventArgs = CreateEventWrittenEventArgs(default, default);
            sut.GetEventTime(eventArgs).Should().Be(EventTime.Unrecognized);
        }

        private EventTimer CreateTimer(int startEventId, int endEventId)
        {
            return new EventTimer(
                new MemoryCache(Options.Create(new MemoryCacheOptions())),
                startEventId,
                endEventId,
                x => (long)x.Payload[0],
                Sampler.Default);
        }

        private EventWrittenEventArgs CreateEventWrittenEventArgs(
            long eventId, DateTime timeStamp)
        {
            var eventArgs = (EventWrittenEventArgs)typeof(EventWrittenEventArgs).CreateInstance(new[] { typeof(EventSource) }, Flags.NonPublic | Flags.Instance, new object[] { null });
            var payload = new ReadOnlyCollection<object>(new List<object>()
            {
                eventId
            });
            eventArgs.SetPropertyValue("Payload", payload);
            eventArgs.SetPropertyValue("TimeStamp", timeStamp);

            return eventArgs;
        }
    }
}
