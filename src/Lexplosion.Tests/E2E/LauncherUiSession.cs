using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using FlaApplication = FlaUI.Core.Application;

namespace Lexplosion.Tests.E2E
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

        private LauncherUiSession(FlaApplication application, bool ownsProcess)
        {
            _application = application;
            _ownsProcess = ownsProcess;
            _automation = new UIA3Automation();
            _artifactsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "lexplosion-data", "Diagnostics", "E2E",
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(_artifactsDirectory);
            _reportFile = Path.Combine(_artifactsDirectory, "navigation.tsv");
        }

        public string ArtifactsDirectory => _artifactsDirectory;

        public static LauncherUiSession Start()
        {
            // Do not kill or restart a launcher owned by the user.
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

            var session = new LauncherUiSession(application, ownsProcess);
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
        }

        public void CaptureFailure(string testName)
        {
            try
            {
                Window window = FindMainWindow();
                if (window == null) return;

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
