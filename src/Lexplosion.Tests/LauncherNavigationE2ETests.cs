using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using Lexplosion.Tests.E2E;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Lexplosion.Tests
{
    /// <summary>
    /// FlaUI.UIA3 smoke tests against an interactive Windows desktop.
    /// Inventories all controls, but selects only tagged navigation and tabs.
    /// Never invokes install, launch, delete, login or payment actions.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    [Category("E2E")]
    public sealed class LauncherNavigationE2ETests
    {
        private LauncherUiSession _session;

        [OneTimeSetUp]
        public void StartLauncher()
        {
            if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E") != "1")
                Assert.Ignore("Set LEXPLOSION_E2E=1 to run interactive UI tests.");

            _session = LauncherUiSession.Start();
            TestContext.Progress.WriteLine("FlaUI UIA3 ready. Artifacts: " + _session.ArtifactsDirectory);
        }

        [TearDown]
        public void SaveFailureEvidence()
        {
            if (_session == null) return;

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                _session.CaptureFailure(TestContext.CurrentContext.Test.Name);

            _session.AttachReport();
        }

        [OneTimeTearDown]
        public void StopLauncher()
        {
            _session?.Dispose();
        }

        [Test]
        public void InventoryVisibleInteractiveControls()
        {
            Window window = _session.WaitForMainWindow();
            var controls = window.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.Button)
                    .Or(cf.ByControlType(ControlType.RadioButton))
                    .Or(cf.ByControlType(ControlType.TabItem))
                    .Or(cf.ByControlType(ControlType.CheckBox))
                    .Or(cf.ByControlType(ControlType.ComboBox))
                    .Or(cf.ByControlType(ControlType.Hyperlink)));

            var visible = controls.Where(IsVisible).ToArray();
            foreach (var group in visible.GroupBy(element => element.ControlType.ToString()))
                TestContext.Progress.WriteLine("FlaUI inventory: " + group.Key + " = " + group.Count());

            _session.Record("inventory.controls", "visible", 0, visible.Length.ToString());
            Assert.That(visible.Length, Is.GreaterThan(0),
                "No interactive controls in main window. Check authorization/splash.");
        }

        [Test]
        public void TraverseNavigationAndTabsWithoutSideEffects()
        {
            Window window = _session.WaitForMainWindow();
            var sidebarIds = VisibleSidebarItems(window)
                .Select(item => item.AutomationId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (sidebarIds.Length == 0)
                Assert.Ignore("No sidebar navigation visible. Account authorization may be needed.");

            var failures = new List<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            int tabsVisited = 0;

            foreach (var id in sidebarIds)
            {
                try
                {
                    window = _session.WaitForMainWindow();
                    var item = Retry.WhileNull(
                        () => FindSidebarItem(window, id),
                        timeout: TimeSpan.FromSeconds(5),
                        interval: TimeSpan.FromMilliseconds(100),
                        ignoreException: true).Result;

                    if (item == null || !IsVisible(item)) continue;

                    SelectSidebar(window, id, item);
                    visited.Add(id);
                    tabsVisited += TraverseVisibleTabs(_session.WaitForMainWindow());
                }
                catch (Exception ex)
                {
                    failures.Add(id + ": " + ex.GetType().Name);
                    _session.Record("navigation.sidebar", id, 0, "error." + ex.GetType().Name);
                    TestContext.Progress.WriteLine("FlaUI navigation error: " + id + " " + ex);
                }
            }

            TestContext.Progress.WriteLine("FlaUI navigation: sidebar=" + visited.Count +
                ", tab selections=" + tabsVisited + ", errors=" + failures.Count);
            Assert.That(visited.Count, Is.GreaterThan(0), "No sidebar items could be selected.");
            Assert.That(failures, Is.Empty, "Navigation failed; check E2E report and QA log.");
        }

        private void SelectSidebar(Window window, string id, AutomationElement item)
        {
            var stopwatch = Stopwatch.StartNew();

            // Model RadioButton selection is exposed through UIA selection pattern.
            item.AsRadioButton().IsChecked = true;
            var selected = Retry.WhileNull(
                () =>
                {
                    var current = FindSidebarItem(window, id);
                    return current != null && current.AsRadioButton().IsChecked ? current : null;
                },
                timeout: TimeSpan.FromSeconds(5),
                interval: TimeSpan.FromMilliseconds(100),
                ignoreException: true).Result;

            if (selected == null)
                throw new TimeoutException("Sidebar never became selected: " + id);

            // Wait for input processing without blocking for a hardcoded
            // full transition duration after the selected state is visible.
            Thread.Sleep(120);
            stopwatch.Stop();
            _session.Record("navigation.sidebar", id, stopwatch.ElapsedMilliseconds, "ok");
            TestContext.Progress.WriteLine("FlaUI sidebar " + id + ": " +
                stopwatch.ElapsedMilliseconds + " ms");
        }

        private int TraverseVisibleTabs(Window window)
        {
            // Re-query the UIA tree after every click: WPF rebuilds TabItems
            // when a new DataTemplate materializes.
            var initial = VisibleTabs(window)
                .Select(item => item.AutomationId)
                .Where(id => id.StartsWith("nav.tab.", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            int visited = 0;
            foreach (string id in initial)
            {
                var tab = Retry.WhileNull(
                    () => FindTab(window, id),
                    timeout: TimeSpan.FromSeconds(3),
                    interval: TimeSpan.FromMilliseconds(100),
                    ignoreException: true).Result;

                if (tab == null || !IsVisible(tab)) continue;

                var stopwatch = Stopwatch.StartNew();
                tab.AsTabItem().Select();

                var selected = Retry.WhileNull(
                    () =>
                    {
                        var current = FindTab(window, id);
                        return current != null && current.AsTabItem().IsSelected ? current : null;
                    },
                    timeout: TimeSpan.FromSeconds(5),
                    interval: TimeSpan.FromMilliseconds(100),
                    ignoreException: true).Result;

                if (selected == null)
                    throw new TimeoutException("Tab never became selected: " + id);

                Thread.Sleep(120);
                stopwatch.Stop();
                visited++;
                _session.Record("navigation.tab", id, stopwatch.ElapsedMilliseconds, "ok");
                TestContext.Progress.WriteLine("FlaUI tab " + id + ": " +
                    stopwatch.ElapsedMilliseconds + " ms");
            }
            return visited;
        }

        private static AutomationElement[] VisibleSidebarItems(Window window)
        {
            return window.FindAllDescendants(cf => cf.ByControlType(ControlType.RadioButton))
                .Where(item => IsVisible(item) &&
                    item.AutomationId.StartsWith("nav.sidebar.", StringComparison.Ordinal))
                .ToArray();
        }

        private static AutomationElement[] VisibleTabs(Window window)
        {
            return window.FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem))
                .Where(IsVisible)
                .ToArray();
        }

        private static AutomationElement FindSidebarItem(Window window, string id)
        {
            return window.FindFirstDescendant(cf =>
                cf.ByAutomationId(id).And(cf.ByControlType(ControlType.RadioButton)));
        }

        private static AutomationElement FindTab(Window window, string id)
        {
            return window.FindFirstDescendant(cf =>
                cf.ByAutomationId(id).And(cf.ByControlType(ControlType.TabItem)));
        }

        private static bool IsVisible(AutomationElement element)
        {
            try
            {
                return element.IsEnabled && !element.IsOffscreen;
            }
            catch (Exception)
            {
                // An element can be removed from the UIA tree during navigation.
                return false;
            }
        }
    }
}
