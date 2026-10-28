using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using FlaApplication = FlaUI.Core.Application;

namespace Lexplosion.E2E
{
    /// <summary>
    /// Runs against the real WPF process. Only processes started by this
    /// session may be closed; attaching never changes process ownership.
    /// </summary>
    internal sealed class LauncherUiSession : IDisposable
    {
        private const string MainWindowId = "Lexplosion.MainWindow";
        private readonly FlaApplication _application;
        private readonly UIA3Automation _automation;
        private readonly bool _ownsProcess;
        private readonly string _artifactsDirectory;
        private readonly string _reportFile;
        private readonly DateTime _startedUtc;
        private int _captureIndex;

        private LauncherUiSession(FlaApplication application, bool ownsProcess, DateTime startedUtc)
        {
            _application = application;
            _ownsProcess = ownsProcess;
            _startedUtc = startedUtc;
            _automation = new UIA3Automation();
            _artifactsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "lexplosion-data", "Diagnostics", "E2E",
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(_artifactsDirectory);
            _reportFile = Path.Combine(_artifactsDirectory, "navigation.tsv");
        }

        public string ArtifactsDirectory => _artifactsDirectory;
        public bool OwnsProcess => _ownsProcess;

        // Raw input stress tests need the intended window on top before
        // dispatching physical mouse events to cached screen coordinates.
        public bool ActivateForInteraction() => PrepareWindowCapture(WaitForMainWindow());

        public static LauncherUiSession Start()
        {
            // Do not kill or restart a launcher owned by the user.
            var startedUtc = DateTime.UtcNow;
            var existing = Process.GetProcessesByName("Lexplosion");
            int? attachedProcessId = null;
            try
            {
                foreach (var process in existing)
                {
                    try
                    {
                        if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                        {
                            attachedProcessId = process.Id;
                            break;
                        }
                    }
                    catch (InvalidOperationException) { }
                }
            }
            finally
            {
                foreach (var process in existing)
                    process.Dispose();
            }

            FlaApplication application;
            bool ownsProcess = !attachedProcessId.HasValue;
            if (attachedProcessId.HasValue)
            {
                application = FlaApplication.Attach(attachedProcessId.Value);
            }
            else
            {
                string executable = ResolveExecutable();
                var startInfo = new ProcessStartInfo(executable)
                {
                    WorkingDirectory = Path.GetDirectoryName(executable),
                    UseShellExecute = false
                };
                startInfo.EnvironmentVariables["LEXPLOSION_QA"] = "1";
                application = FlaApplication.Launch(startInfo);
            }

            var session = new LauncherUiSession(application, ownsProcess, startedUtc);
            try
            {
                session.WaitForMainWindow();
                return session;
            }
            catch
            {
                session.Dispose();
                throw;
            }
        }

        public Window WaitForMainWindow()
        {
            var found = Retry.WhileNull(
                FindMainWindow,
                timeout: TimeSpan.FromSeconds(90),
                interval: TimeSpan.FromMilliseconds(250),
                ignoreException: true).Result;

            if (found == null)
                throw new TimeoutException("Lexplosion main window not found in 90 s. " +
                    "Check the splash screen, account authorization and startup log.");

            return found;
        }

        public Window FindMainWindow()
        {
            if (_application.HasExited)
                return null;

            // GetMainWindow can keep pointing to the SplashWindow.
            // The stable AutomationId identifies the real WPF MainWindow.
            return _application.GetAllTopLevelWindows(_automation)
                .FirstOrDefault(window => window.AutomationId == MainWindowId);
        }

        public void Record(string action, string id, long milliseconds, string result)
        {
            // Only deterministic operation names / resource keys are recorded.
            string line = DateTime.UtcNow.ToString("O") + "\t" +
                SafeText(action) + "\t" + SafeText(id) + "\t" +
                milliseconds + "\t" + SafeText(result);
            File.AppendAllText(_reportFile, line + Environment.NewLine);
        }

        public void AttachReport()
        {
            if (File.Exists(_reportFile))
                NUnit.Framework.TestContext.AddTestAttachment(_reportFile, "FlaUI navigation timings");

            var visual = Path.Combine(_artifactsDirectory, "visual-results.tsv");
            if (File.Exists(visual))
                NUnit.Framework.TestContext.AddTestAttachment(visual, "Visual baseline checks");

            WriteCoverageSummary();
        }

        private void WriteCoverageSummary()
        {
            if (!File.Exists(_reportFile)) return;

            var lines = File.ReadAllLines(_reportFile);
            var passed = lines.Count(line => line.Contains("\tPASS"));
            var failed = lines.Count(line => line.Contains("\tFAIL"));
            var skipped = lines.Count(line => line.Contains("\tSKIP"));
            var groups = new[]
            {
                "nav.group.settings", "nav.group.friends", "nav.group.multiplayer",
                "nav.group.instance.overview", "nav.group.instance.settings",
                "nav.group.instance.addons", "nav.group.server.overview",
                "nav.group.instance.share"
            };
            string report = Path.Combine(_artifactsDirectory, "coverage-summary.txt");
            var output = new List<string>
            {
                "Lexplosion dynamic navigation E2E",
                "E2E start UTC: " + _startedUtc.ToString("O"),
                "Passing navigation events: " + passed,
                "Failing navigation events: " + failed,
                "Skipped navigation events: " + skipped,
                "Note: profiles and modals require safe fixture data to become accessible."
            };
            foreach (var group in groups)
            {
                bool discovered = lines.Any(line => line.Contains("\t" + group + "\t") &&
                    line.Contains("\tDISCOVERED")) ||
                    lines.Any(line => line.Contains("\t" + group + "."));
                output.Add(group + ": " + (discovered ? "REACHED" : "NOT COVERED"));
            }

            File.WriteAllLines(report, output);
            NUnit.Framework.TestContext.AddTestAttachment(report, "E2E navigation coverage");
        }

        public void CapturePerformanceLog()
        {
            // Snapshot only this run's records, not unrelated prior sessions.
            var source = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "lexplosion-data", "Diagnostics", "ui-performance.log");
            var destination = Path.Combine(_artifactsDirectory, "ui-performance.log");
            var events = new List<Tuple<DateTime, string, double>>();
            try
            {
                if (File.Exists(source))
                {
                    using (var stream = new FileStream(source, FileMode.Open,
                        FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(destination, false))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split('|');
                            if (parts.Length < 3 ||
                                !DateTime.TryParse(parts[0].Trim(), CultureInfo.InvariantCulture,
                                    DateTimeStyles.RoundtripKind, out var stamp) ||
                                stamp.ToUniversalTime() < _startedUtc)
                                continue;

                            writer.WriteLine(line);
                            string duration = parts[2].Trim().Replace("ms", "").Trim();
                            if (double.TryParse(duration, NumberStyles.Float,
                                CultureInfo.InvariantCulture, out var ms))
                                events.Add(Tuple.Create(stamp, parts[1].Trim(), ms));
                        }
                    }
                }

                if (!File.Exists(destination))
                    File.WriteAllText(destination,
                        "# No QA telemetry recorded in this E2E run. " +
                        "If attaching to an existing launcher, set LEXPLOSION_QA=1 before starting it." +
                        Environment.NewLine);

                string summary = Path.Combine(_artifactsDirectory, "performance-summary.txt");
                var stalls = events.Where(e => e.Item2 == "ui.dispatcher.stall")
                    .Select(e => e.Item3).OrderBy(x => x).ToArray();
                var inputLag = events.Where(e => e.Item2 == "ui.input.lag")
                    .Select(e => e.Item3).OrderBy(x => x).ToArray();
                bool monitorStarted = events.Any(e => e.Item2 == "qa.input.monitor.start");
                var prepare = events.Where(e => e.Item2 == "navigation.page.prepare")
                    .Select(e => e.Item3).OrderBy(x => x).ToArray();
                File.WriteAllLines(summary, new[]
                {
                    "Source: " + source,
                    "E2E started UTC: " + _startedUtc.ToString("O"),
                    "Telemetry status: " + (events.Count == 0 ? "NO DATA" : "AVAILABLE"),
                    "Telemetry records: " + events.Count,
                    "UI stalls > threshold: " + stalls.Length,
                    "UI stall max ms: " + (stalls.Length == 0 ? "n/a" : stalls.Last().ToString("F1", CultureInfo.InvariantCulture)),
                    "UI stall p95 ms: " + Percentile(stalls, 0.95),
                    "Input probe status: " + (monitorStarted ? "ACTIVE" : "NOT DETECTED"),
                    "UI input lag count: " + inputLag.Length,
                    "UI input lag max ms: " + (inputLag.Length == 0 ? "n/a" :
                        inputLag.Last().ToString("F1", CultureInfo.InvariantCulture)),
                    "UI input lag p95 ms: " + Percentile(inputLag, 0.95),
                    "Page prepare count: " + prepare.Length,
                    "Page prepare p95 ms: " + Percentile(prepare, 0.95),
                    "Input lag is input-priority queue delay; legacy stall is Background timer lateness; neither is GPU frame time."
                });
                NUnit.Framework.TestContext.AddTestAttachment(summary, "E2E performance summary");
                if (File.Exists(destination))
                    NUnit.Framework.TestContext.AddTestAttachment(destination, "E2E filtered ui-performance.log");
            }
            catch (IOException ex)
            {
                NUnit.Framework.TestContext.Progress.WriteLine(
                    "Performance log read failed: " + ex.Message);
            }
        }

        private static string Percentile(double[] sorted, double fraction)
        {
            if (sorted.Length == 0) return "n/a";
            int index = (int)Math.Ceiling(fraction * sorted.Length) - 1;
            return sorted[Math.Max(0, Math.Min(index, sorted.Length - 1))]
                .ToString("F1", CultureInfo.InvariantCulture);
        }

        public void CaptureState(string route, bool compareBaseline)
        {
            Window window = WaitForMainWindow();
            // FlaUI captures screen pixels: activate the real window and
            // wait for its bounds to settle, otherwise other windows can leak
            // into screenshots during startup, resize and rapid navigation.
            if (!PrepareWindowCapture(window))
            {
                MarkUnverifiedCapture(route);
                return;
            }
            string file = (++_captureIndex).ToString("D3") + "-" +
                SafeText(route) + ".png";
            string path = Path.Combine(_artifactsDirectory, file);
            Capture.Element(window).ToFile(path);
            NUnit.Framework.TestContext.AddTestAttachment(path, "UI state: " + route);

            if (compareBaseline)
                VisualSnapshots.Verify(path, route, _artifactsDirectory);
        }

        public void CaptureFailure(string testName)
        {
            try
            {
                Window window = FindMainWindow();
                if (window == null) return;
                if (!PrepareWindowCapture(window))
                {
                    MarkUnverifiedCapture("failure-" + testName);
                    return;
                }

                string path = Path.Combine(_artifactsDirectory, SafeText(testName) + ".png");
                Capture.Element(window).ToFile(path);
                NUnit.Framework.TestContext.AddTestAttachment(path, "FlaUI screenshot on failure");
                NUnit.Framework.TestContext.Progress.WriteLine("FlaUI screenshot: " + path);
            }
            catch (Exception e)
            {
                NUnit.Framework.TestContext.Progress.WriteLine(
                    "FlaUI could not capture screenshot: " + e.GetType().Name);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private bool PrepareWindowCapture(Window window)
        {
            var expected = window.Properties.NativeWindowHandle.ValueOrDefault;
            if (expected == IntPtr.Zero) return false;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                // FlaUI's capture is a screen rectangle, not a WPF render.
                // Focus alone may not bring the window in front on Windows.
                window.SetForeground();
                var before = window.BoundingRectangle;
                Thread.Sleep(150);
                if (GetForegroundWindow() == expected &&
                    window.BoundingRectangle.Equals(before) &&
                    before.Width >= 500 && before.Height >= 300)
                    return true;
            }

            return false;
        }

        private void MarkUnverifiedCapture(string route)
        {
            string message = "Window not foreground/stable; visual snapshot skipped: " + route;
            NUnit.Framework.TestContext.Progress.WriteLine(message);
            File.AppendAllText(Path.Combine(_artifactsDirectory, "visual-results.tsv"),
                SafeText(route) + "\tUNVERIFIED\twindow-not-foreground-or-unstable" +
                Environment.NewLine);
            if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E_STRICT_VISUAL") == "1")
                NUnit.Framework.Assert.Fail(message);
        }

        private static string SafeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            return new string(value.Take(80)
                .Select(ch => char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '_'
                    ? ch : '_').ToArray());
        }

        private static string ResolveExecutable()
        {
            string explicitPath = Environment.GetEnvironmentVariable("LEXPLOSION_E2E_EXE");
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                if (!File.Exists(explicitPath))
                    throw new FileNotFoundException("LEXPLOSION_E2E_EXE not found.", explicitPath);
                return Path.GetFullPath(explicitPath);
            }

            var directory = new DirectoryInfo(NUnit.Framework.TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                string sourceRoot = Directory.Exists(Path.Combine(directory.FullName, "Lexplosion.UI.WPF"))
                    ? directory.FullName
                    : Path.Combine(directory.FullName, "src");

                string project = Path.Combine(sourceRoot, "Lexplosion.UI.WPF");
                if (Directory.Exists(project))
                {
                    foreach (string config in new[] { "Debug", "Release" })
                    {
                        foreach (string output in new[]
                        {
                            Path.Combine("net472", "win-x86"),
                            "net472",
                            "win-x86",
                            ""
                        })
                        {
                            string candidate = Path.Combine(project, "bin", config,
                                output, "Lexplosion.exe");
                            if (File.Exists(candidate))
                                return candidate;
                        }
                    }
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Build Lexplosion.UI.WPF or set LEXPLOSION_E2E_EXE.");
        }

        public void Dispose()
        {
            try
            {
                if (_ownsProcess && !_application.HasExited)
                {
                    // Lexplosion can minimize to tray instead of exiting.
                    _application.Close(false);
                    if (!_application.HasExited)
                        _application.Kill();
                }
            }
            catch (InvalidOperationException) { }
            finally
            {
                _automation.Dispose();
                _application.Dispose();
            }
        }
    }
}
