using System;
using System.IO;
using System.Threading;
using Lexplosion.QA;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class PerformanceTelemetryTests
    {
        [Test]
        public void EventLineContainsStableUtcTimestampDurationAndThread()
        {
            var timestamp = new DateTime(2026, 10, 28, 10, 20, 30, DateTimeKind.Utc);
            var entry = new PerformanceEvent(timestamp, "navigation.sidebar.commit", 42.86, 17);
            string line = entry.ToLogLine();

            Assert.That(line, Does.Contain("2026-10-28T10:20:30"));
            Assert.That(line, Does.Contain("navigation.sidebar.commit"));
            Assert.That(line, Does.Contain("42.9ms"));
            Assert.That(line, Does.Contain("thread=17"));
        }

        [Test]
        public void EventEscapesUnexpectedOperationCharacters()
        {
            var entry = new PerformanceEvent(DateTime.UtcNow, "pages\nprivate@example.com", 5, 1);
            Assert.That(entry.Operation, Is.EqualTo("pages_private_example.com"));
            Assert.That(entry.ToLogLine().Split('\n').Length, Is.EqualTo(1));
        }

        [Test]
        public void DisabledTelemetryDoesNotCreateFiles()
        {
            string path = Path.Combine(Path.GetTempPath(), "lexplosion-qa-disabled-" + Guid.NewGuid() + ".log");
            PerformanceTelemetry.Configure(true, path);
            PerformanceTelemetry.Configure(false, null);
            using (PerformanceTelemetry.Measure("test.disabled"))
            {
                PerformanceTelemetry.Record("test.disabled", 900);
            }
            Assert.That(File.Exists(path), Is.False);
        }

        [Test]
        public void EnabledTelemetryWritesLocalTimingAsynchronously()
        {
            string path = Path.Combine(Path.GetTempPath(), "lexplosion-qa-" + Guid.NewGuid() + ".log");
            try
            {
                PerformanceTelemetry.Configure(true, path);
                PerformanceTelemetry.Record("test.event", 25.5, 20);
                PerformanceTelemetry.Record("test.below-threshold", 1, 20);

                Assert.That(SpinWait.SpinUntil(() =>
                {
                    try
                    {
                        return File.Exists(path) && File.ReadAllText(path).Contains("test.event");
                    }
                    catch (IOException)
                    {
                        return false;
                    }
                }, 4000), Is.True);
                string log = File.ReadAllText(path);
                Assert.That(log, Does.Contain("test.event"));
                Assert.That(log, Does.Not.Contain("test.below-threshold"));
            }
            finally
            {
                PerformanceTelemetry.Configure(false, null);
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
