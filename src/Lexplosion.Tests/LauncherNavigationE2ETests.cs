using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    /// <summary>
    /// Safe, opt-in E2E smoke test. Attaches to an existing launcher or launches
    /// its WPF executable automatically, then waits for its main window.
    /// It never performs destructive or authenticated actions.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    [Category("E2E")]
    public sealed class LauncherNavigationE2ETests
    {
        private const int SettleMilliseconds = 320;
        private static Process _launcher;
        private static bool _ownsLauncher;

        [OneTimeSetUp]
        public void StartLauncher()
        {
            if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E") != "1")
                Assert.Ignore("Opt-in required: set LEXPLOSION_E2E=1.");

            var existing = Process.GetProcessesByName("Lexplosion");
            try
            {
                // Attach without terminating a user's already running launcher.
                _launcher = existing.FirstOrDefault(p =>
                {
                    try { return !p.HasExited && p.MainWindowHandle != IntPtr.Zero; }
                    catch (InvalidOperationException) { return false; }
                });

                if (_launcher == null)
                {
                    string executable = ResolveExecutable();
                    var startInfo = new ProcessStartInfo(executable)
                    {
                        WorkingDirectory = Path.GetDirectoryName(executable),
                        UseShellExecute = false
                    };
                    // Always capture startup stalls when E2E owns the process.
                    startInfo.EnvironmentVariables["LEXPLOSION_QA"] = "1";
                    _launcher = Process.Start(startInfo);
                    _ownsLauncher = true;
                    TestContext.Progress.WriteLine("E2E launched: " + executable);
                }
                else
                {
                    TestContext.Progress.WriteLine("E2E attached to existing Lexplosion process");
                }
            }
            finally
            {
                foreach (var process in existing)
                    if (!ReferenceEquals(process, _launcher)) process.Dispose();
            }
        }

        [OneTimeTearDown]
        public void StopLauncher()
        {
            if (_launcher == null) return;

            try
            {
                if (_ownsLauncher && !_launcher.HasExited)
                {
                    _launcher.CloseMainWindow();
                    if (!_launcher.WaitForExit(3000))
                        _launcher.Kill();
                }
            }
            catch (InvalidOperationException) { }
            finally
            {
                _launcher.Dispose();
                _launcher = null;
                _ownsLauncher = false;
            }
        }

        private static string ResolveExecutable()
        {
            string explicitPath = Environment.GetEnvironmentVariable("LEXPLOSION_E2E_EXE");
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                if (File.Exists(explicitPath))
                    return Path.GetFullPath(explicitPath);

                Assert.Fail("LEXPLOSION_E2E_EXE points to a missing file: " + explicitPath);
            }

            // Prefer the actual WPF project output, with all its bundled assets,
            // over a copied test-output EXE that might lack runtime content.
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                string src = Directory.Exists(Path.Combine(directory.FullName, "Lexplosion.UI.WPF"))
                    ? directory.FullName
                    : Path.Combine(directory.FullName, "src");

                if (Directory.Exists(Path.Combine(src, "Lexplosion.UI.WPF")))
                {
                    foreach (string configuration in new[] { "Debug", "Release" })
                    {
                        foreach (string relativeOutput in new[]
                        {
                            Path.Combine("net472", "win-x86"),
                            "net472",
                            "win-x86",
                            ""
                        })
                        {
                            string candidate = Path.Combine(src, "Lexplosion.UI.WPF",
                                "bin", configuration, relativeOutput, "Lexplosion.exe");
                            if (File.Exists(candidate))
                                return candidate;
                        }
                    }
                }
                directory = directory.Parent;
            }

            Assert.Fail("Lexplosion.exe not found. Build the WPF project or set LEXPLOSION_E2E_EXE.");
            return null;
        }

        [Test]
        public void InventoryAllVisibleInteractiveControls()
        {
            AutomationElement window = FindMainWindow();
            var controls = FindInteractiveControls(window);

            var counts = controls.Cast<AutomationElement>()
                .Where(IsVisible)
                .GroupBy(x => x.Current.ControlType.ProgrammaticName)
                .OrderBy(x => x.Key);

            foreach (var type in counts)
                TestContext.Progress.WriteLine("E2E inventory: " + type.Key + " = " + type.Count());

            Assert.That(controls.Count, Is.GreaterThan(0),
                "No interactive controls found. The launcher may still be on its splash screen.");

            // Inventory includes unsafe controls but never activates them.
            TestContext.Progress.WriteLine(
                "E2E inventory is read-only: installation, deletion, authentication and game launch are skipped.");
        }

        [Test]
        public void TraverseSidebarAndVisibleTabsWithoutDestructiveActions()
        {
            AutomationElement window = FindMainWindow();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var failures = new List<string>();
            int selectedTabs = 0;

            // Only radio buttons explicitly tagged as sidebar navigation are
            // safe to click automatically. The launcher may have more buttons.
            var sidebarIds = DiscoverSidebarIds(window);
            if (sidebarIds.Count == 0)
                Assert.Ignore("No sidebar is visible. Open the main menu or an instance profile first.");

            foreach (string id in sidebarIds)
            {
                var current = FindSidebar(window, id);
                if (current == null || !IsVisible(current)) continue;

                TrySelect(current, "sidebar", failures);
                visited.Add(id);
                Thread.Sleep(SettleMilliseconds);
                selectedTabs += TraverseCurrentTabs(window, failures);
            }

            TestContext.Progress.WriteLine(
                "E2E result: sidebar=" + visited.Count +
                ", tabs=" + selectedTabs + ", failures=" + failures.Count);

            foreach (var failure in failures)
                TestContext.Progress.WriteLine("E2E failure: " + failure);

            Assert.That(visited.Count, Is.GreaterThan(0));
            Assert.That(failures, Is.Empty,
                "Navigation smoke test had errors. See the NUnit output and UI performance log.");
        }

        private static AutomationElement FindMainWindow()
        {
            if (_launcher == null)
                Assert.Fail("E2E launcher was not initialized.");

            // Lexplosion starts with a SplashWindow. MainWindowHandle can
            // point at that splash even after the real window appears.
            var timeout = Stopwatch.StartNew();
            while (timeout.Elapsed < TimeSpan.FromSeconds(90))
            {
                try
                {
                    if (_launcher.HasExited)
                        Assert.Fail("Lexplosion exited before its main window was ready.");

                    var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ProcessIdProperty, _launcher.Id));

                    foreach (AutomationElement window in windows)
                    {
                        if (window.Current.ControlType == ControlType.Window &&
                            window.Current.Name.Equals("Lexplosion", StringComparison.OrdinalIgnoreCase))
                            return window;
                    }
                }
                catch (ElementNotAvailableException)
                {
                    // The splash can close while the Automation tree is queried.
                }

                Thread.Sleep(250);
            }

            Assert.Fail("No Lexplosion main window after 90 s. Check startup logs or authentication.");
            return null;
        }

        private static AutomationElementCollection FindInteractiveControls(AutomationElement window)
        {
            return window.FindAll(TreeScope.Descendants, new OrCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.CheckBox),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Hyperlink)));
        }

        private static List<string> DiscoverSidebarIds(AutomationElement window)
        {
            return window.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton))
                .Cast<AutomationElement>()
                .Where(IsVisible)
                .Select(item => item.Current.AutomationId)
                .Where(id => id != null && id.StartsWith("nav.sidebar.", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static AutomationElement FindSidebar(AutomationElement window, string id)
        {
            return window.FindFirst(TreeScope.Descendants, new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton),
                new PropertyCondition(AutomationElement.AutomationIdProperty, id)));
        }

        private static int TraverseCurrentTabs(AutomationElement window, List<string> failures)
        {
            // Tabs may be reconstructed after each selection. Re-discover them,
            // and never click arbitrary Buttons or ToggleButtons.
            var initial = window.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
            int visited = 0;

            for (int i = 0; i < initial.Count; i++)
            {
                var tabs = window.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                if (i >= tabs.Count) break;
                var tab = tabs[i];
                if (!IsVisible(tab)) continue;

                TrySelect(tab, "tab", failures);
                visited++;
                Thread.Sleep(SettleMilliseconds);
            }
            return visited;
        }

        private static void TrySelect(AutomationElement element, string kind, List<string> failures)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object pattern))
                    ((SelectionItemPattern)pattern).Select();
                else if (element.TryGetCurrentPattern(InvokePattern.Pattern, out pattern))
                    ((InvokePattern)pattern).Invoke();
                else
                {
                    failures.Add(kind + ": no selection automation pattern");
                    return;
                }

                sw.Stop();
                TestContext.Progress.WriteLine("E2E " + kind + " selected in " +
                    sw.ElapsedMilliseconds + " ms");
            }
            catch (ElementNotAvailableException)
            {
                // Navigation can rebuild the tree before the next action.
                TestContext.Progress.WriteLine("E2E " + kind + ": element replaced during navigation");
            }
            catch (Exception e)
            {
                failures.Add(kind + ": " + e.GetType().Name);
            }
        }

        private static bool IsVisible(AutomationElement element)
        {
            try
            {
                return !element.Current.IsOffscreen && element.Current.IsEnabled;
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
        }
    }
}
