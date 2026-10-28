using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using NUnit.Framework;

namespace Lexplosion.Tests
{
    /// <summary>
    /// Safe E2E smoke test for a *running, authenticated* launcher.
    /// It audits every visible interactive control, but activates navigation
    /// items only. Never clicks install, launch, delete, login or payment buttons.
    /// Run explicitly with LEXPLOSION_E2E=1 in the same Windows desktop session.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    [Category("E2E")]
    public sealed class LauncherNavigationE2ETests
    {
        private const int SettleMilliseconds = 320;

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
            if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E") != "1")
                Assert.Ignore("Opt-in required: set LEXPLOSION_E2E=1.");

            foreach (var process in Process.GetProcessesByName("Lexplosion"))
            {
                try
                {
                    if (process.MainWindowHandle == IntPtr.Zero) continue;
                    var window = AutomationElement.FromHandle(process.MainWindowHandle);
                    if (window != null &&
                        window.Current.ControlType == ControlType.Window &&
                        window.Current.Name.IndexOf("Lexplosion", StringComparison.OrdinalIgnoreCase) >= 0)
                        return window;
                }
                catch (ElementNotAvailableException) { }
                catch (InvalidOperationException) { }
                finally { process.Dispose(); }
            }

            Assert.Ignore("Start Lexplosion and open its main window before running E2E.");
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
