using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Lexplosion.E2E
{
    public enum NavigationMode { Normal, Reverse, Shuffle, Rapid }

    /// <summary>
    /// Exercises all navigation elements currently exposed by the application.
    /// Never invokes unknown buttons or commands with side effects.
    /// Each unavailable route is reported as SKIP rather than marked PASS.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    [Category("E2E")]
    public sealed class LauncherNavigationE2ETests
    {
        private LauncherUiSession _session;
        private const int MaxActions = 250;

        [OneTimeSetUp]
        public void SetUp()
        {
            if (Environment.GetEnvironmentVariable("LEXPLOSION_E2E") != "1")
                Assert.Ignore("Set LEXPLOSION_E2E=1 to start live UI tests.");

            _session = LauncherUiSession.Start();
        }

        [TearDown]
        public void SaveDiagnostics()
        {
            if (_session == null) return;

            try
            {
                if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                    _session.CaptureFailure(TestContext.CurrentContext.Test.Name);
                _session.AttachReport();
                _session.CapturePerformanceLog();
            }
            catch (Exception e)
            {
                TestContext.Progress.WriteLine("E2E report error: " + e);
            }
        }

        [OneTimeTearDown]
        public void TearDown() => _session?.Dispose();

        [Test, Order(0)]
        public void InventoryAllVisibleControlsAndNavigation()
        {
            Window window = _session.WaitForMainWindow();
            var all = window.FindAllDescendants().Where(Visible).ToArray();
            var interactive = all.Where(e => e.ControlType == ControlType.Button ||
                e.ControlType == ControlType.RadioButton ||
                e.ControlType == ControlType.TabItem ||
                e.ControlType == ControlType.CheckBox ||
                e.ControlType == ControlType.ComboBox ||
                e.ControlType == ControlType.Hyperlink).ToArray();

            Assert.That(window.BoundingRectangle.Width, Is.GreaterThan(500),
                "Main window is hidden or too small to verify.");
            Assert.That(interactive.Length, Is.GreaterThan(0), "No interactive controls accessible.");

            foreach (var group in interactive.GroupBy(e => e.ControlType.ToString()))
                TestContext.Progress.WriteLine("E2E inventory " + group.Key + ": " + group.Count());

            int safe = interactive.Count(e => e.AutomationId.StartsWith("nav.", StringComparison.Ordinal));
            _session.Record("inventory", "interactive", 0, interactive.Length.ToString());
            _session.Record("inventory", "safe-nav", 0, safe.ToString());
            TestContext.Progress.WriteLine("Safe navigation IDs accessible: " + safe +
                ". Unmarked buttons were inventoried, NOT clicked.");
        }

        [TestCase(NavigationMode.Normal, Order = 20)]
        [TestCase(NavigationMode.Reverse, Order = 30)]
        [TestCase(NavigationMode.Shuffle, Order = 40)]
        public void TraverseAvailableNavigationAndVerifyRenderedState(NavigationMode mode)
        {
            var window = _session.WaitForMainWindow();
            var initial = SidebarIds(window);
            if (initial.Length == 0)
            {
                _session.Record("coverage.skip", "sidebar.not-accessible", 0, "SKIP");
                Assert.Ignore("No sidebar reachable: login, first-run wizard or unavailable content.");
            }

            var failures = new List<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            int actions = 0;
            int tabs = 0;

            foreach (string id in Order(initial, mode))
            {
                if (actions >= MaxActions) break;
                if (!TrySelectSidebar(id, mode, failures)) continue;
                visited.Add(id);
                actions++;

                // Re-discover tab groups after the new page has materialized.
                var visitedGroups = new HashSet<string>(StringComparer.Ordinal);
                TraverseTabGroups(mode, visitedGroups, failures, ref tabs, ref actions, 0);
            }

            foreach (string skipped in initial.Except(visited))
                _session.Record("coverage.skip", skipped, 0, "SKIP");

            _session.Record("coverage", mode.ToString(), actions, "sidebar=" + visited.Count + ".tabs=" + tabs);
            TestContext.Progress.WriteLine("E2E " + mode + " sidebar=" + visited.Count +
                "/" + initial.Length + ", tabs=" + tabs + ", failures=" + failures.Count);

            Assert.That(actions, Is.GreaterThan(0), "No navigation action executed.");
            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test, Order(50)]
        public void RapidSwitchingPreservesTheFinalSelection()
        {
            var ids = SidebarIds(_session.WaitForMainWindow());
            if (ids.Length < 2)
            {
                _session.Record("coverage.skip", "rapid.sidebar", 0, "SKIP");
                Assert.Ignore("Need at least two accessible sidebar destinations.");
            }

            // No sleeps between actions. Test exactly the problem where
            // heavy page creation races with indicator motion.
            var sequence = new List<string>();
            for (int round = 0; round < 4; round++)
                sequence.AddRange(round % 2 == 0 ? ids : ids.Reverse());

            sequence = sequence.Take(60).ToList();
            var watch = Stopwatch.StartNew();
            string last = null;
            int clicked = 0;
            foreach (string id in sequence)
            {
                var element = FindSidebar(_session.WaitForMainWindow(), id);
                if (element == null || !Visible(element))
                {
                    _session.Record("coverage.skip", id, 0, "SKIP");
                    continue;
                }
                element.AsRadioButton().IsChecked = true;
                last = id;
                clicked++;
            }
            watch.Stop();

            if (last == null) Assert.Ignore("No sidebar buttons available for stress test.");
            Assert.That(WaitSelected(() => FindSidebar(_session.WaitForMainWindow(), last),
                e => e.AsRadioButton().IsChecked, TimeSpan.FromSeconds(10)), Is.True,
                "Rapid clicking lost the last sidebar destination: " + last);

            AssertVisibleWindow();
            _session.Record("stress.sidebar", last, watch.ElapsedMilliseconds,
                "PASS.count=" + clicked);
            _session.CaptureState("stress-sidebar-final-" + last, false);

            // Revisit every accessible sidebar section to stress its tab groups.
            int tested = 0;
            foreach (string sidebarId in ids)
            {
                var sidebar = FindSidebar(_session.WaitForMainWindow(), sidebarId);
                if (sidebar == null || !Visible(sidebar)) continue;
                sidebar.AsRadioButton().IsChecked = true;
                if (!WaitSelected(() => FindSidebar(_session.WaitForMainWindow(), sidebarId),
                    e => e.AsRadioButton().IsChecked, TimeSpan.FromSeconds(8))) continue;
                Thread.Sleep(200);

                foreach (string groupId in TabGroupIds(_session.WaitForMainWindow()))
                {
                    var items = TabIds(groupId);
                    if (items.Length < 2) continue;

                    var clicks = Enumerable.Range(0, 3).SelectMany(n =>
                        n % 2 == 0 ? items : items.Reverse()).ToArray();
                    string final = null;
                    var timer = Stopwatch.StartNew();
                    foreach (string tabId in clicks)
                    {
                        var target = FindTab(groupId, tabId);
                        if (target == null || !Visible(target)) continue;
                        target.AsTabItem().Select();
                        final = tabId;
                    }
                    timer.Stop();

                    if (final == null) continue;
                    bool isSelected = WaitSelected(() => FindTab(groupId, final),
                        e => e.AsTabItem().IsSelected, TimeSpan.FromSeconds(10));
                    _session.Record("stress.tabs", groupId + "." + final,
                        timer.ElapsedMilliseconds, isSelected ? "PASS" : "FAIL");
                    Assert.That(isSelected, Is.True, "Rapid tab selection lost " + groupId);
                    tested++;
                }
            }

            if (tested == 0)
                _session.Record("coverage.skip", "stress.tabs.unavailable", 0, "SKIP");
        }

        [Test, Order(1000)]
        public void DispatcherPerformanceDoesNotExceedQaBudget()
        {
            // An actual profiler is still needed to attribute CPU stacks.
            // This test makes missing QA data and severe UI starvation visible
            // as failures, not just interesting numbers in an attachment.
            Thread.Sleep(200);
            _session.CapturePerformanceLog();
            string report = Path.Combine(_session.ArtifactsDirectory, "performance-summary.txt");
            Assert.That(File.Exists(report), Is.True, "Missing performance report.");

            var lines = File.ReadAllLines(report);
            string status = lines.FirstOrDefault(l => l.StartsWith("Telemetry status: "));
            Assert.That(status, Is.EqualTo("Telemetry status: AVAILABLE"),
                "No QA data collected. Launch Lexplosion with LEXPLOSION_QA=1.");

            double threshold = ReadBudget("LEXPLOSION_E2E_MAX_STALL_MS", 700);
            string worst = lines.FirstOrDefault(l => l.StartsWith("UI stall max ms: "));
            var raw = (worst ?? "").Replace("UI stall max ms: ", "");
            if (raw == "n/a") return; // No threshold-exceeding UI stalls at all.

            Assert.That(double.TryParse(raw, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double maxStall), Is.True);
            _session.Record("performance.budget", "ui.dispatcher.stall",
                (long)Math.Ceiling(maxStall), maxStall <= threshold ? "PASS" : "FAIL");
            Assert.That(maxStall, Is.LessThanOrEqualTo(threshold),
                "UI Dispatcher exceeded " + threshold + " ms. Check attached ui-performance.log.");
        }

        private static double ReadBudget(string envKey, double fallback)
        {
            string raw = Environment.GetEnvironmentVariable(envKey);
            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture,
                out double value) && value >= 0 ? value : fallback;
        }

        [Test, Order(10)]
        public void OpenReachableInstanceAndServerProfiles()
        {
            var failures = new List<string>();
            int visited = 0;

            // Only explicitly tagged safe entry points are allowed here.
            foreach (var route in new[]
            {
                new { Sidebar = "nav.sidebar.Library", Entry = "nav.open.instance",
                    Back = "nav.back.instance", Name = "instance.library" },
                new { Sidebar = "nav.sidebar.Catalog", Entry = "nav.open.instance",
                    Back = "nav.back.instance", Name = "instance.catalog" },
                new { Sidebar = "nav.sidebar.Multiplayer", Entry = "nav.open.server",
                    Back = "nav.back.server", Name = "server.multiplayer" }
            })
            {
                try
                {
                    var window = _session.WaitForMainWindow();
                    var sidebar = FindSidebar(window, route.Sidebar);
                    if (sidebar == null || !Visible(sidebar))
                    {
                        _session.Record("coverage.skip", route.Name, 0, "SKIP.no-sidebar");
                        continue;
                    }

                    sidebar.AsRadioButton().IsChecked = true;
                    if (!WaitSelected(() => FindSidebar(_session.WaitForMainWindow(), route.Sidebar),
                        e => e.AsRadioButton().IsChecked, TimeSpan.FromSeconds(8)))
                        throw new TimeoutException("Unable to reach " + route.Sidebar);

                    Thread.Sleep(450);
                    if (route.Entry == "nav.open.server")
                    {
                        var tab = FindTab("nav.group.multiplayer", "nav.tab.PartnerServers");
                        if (tab == null)
                        {
                            _session.Record("coverage.skip", route.Name, 0, "SKIP.no-server-tab");
                            continue;
                        }
                        tab.AsTabItem().Select();
                        Thread.Sleep(450);
                    }

                    var entry = _session.WaitForMainWindow().FindFirstDescendant(cf =>
                        cf.ByAutomationId(route.Entry));
                    if (entry == null || !Visible(entry))
                    {
                        _session.Record("coverage.skip", route.Name, 0, "SKIP.no-cards");
                        continue;
                    }

                    var watch = Stopwatch.StartNew();
                    // For instances this is the logo Grid with a MouseDown handler;
                    // for servers this is a Button with OpenServerPageCommand.
                    entry.Click();
                    var back = Retry.WhileNull(() =>
                        _session.WaitForMainWindow().FindFirstDescendant(cf =>
                            cf.ByAutomationId(route.Back)),
                        timeout: TimeSpan.FromSeconds(10),
                        interval: TimeSpan.FromMilliseconds(150),
                        ignoreException: true).Result;
                    if (back == null || !Visible(back))
                        throw new TimeoutException("Profile did not open: " + route.Name);

                    Thread.Sleep(450);
                    AssertVisibleWindow();
                    watch.Stop();
                    _session.Record("navigation.profile", route.Name, watch.ElapsedMilliseconds, "PASS");
                    _session.CaptureState("profile-" + route.Name, true);

                    int tabs = 0;
                    int actions = 0;
                    TraverseTabGroups(NavigationMode.Normal,
                        new HashSet<string>(StringComparer.Ordinal), failures,
                        ref tabs, ref actions, 0);
                    visited++;
                }
                catch (Exception ex)
                {
                    failures.Add(route.Name + ": " + ex.Message);
                    _session.Record("navigation.profile", route.Name, 0,
                        "FAIL." + ex.GetType().Name);
                }
                finally
                {
                    // Return only through explicitly tagged Back controls.
                    var current = _session.FindMainWindow();
                    var back = current?.FindFirstDescendant(cf =>
                        cf.ByAutomationId(route.Back));
                    if (back != null && Visible(back))
                    {
                        try { back.AsButton().Invoke(); }
                        catch (Exception ex)
                        {
                            failures.Add(route.Name + ".back: " + ex.Message);
                        }
                    }
                }
            }

            _session.Record("coverage", "profile", visited, "visited");
            if (visited == 0 && failures.Count == 0)
                Assert.Ignore("No safely openable instance/server profiles in current fixture.");

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        private bool TrySelectSidebar(string id, NavigationMode mode, List<string> failures)
        {
            try
            {
                var target = FindSidebar(_session.WaitForMainWindow(), id);
                if (target == null || !Visible(target))
                {
                    _session.Record("coverage.skip", id, 0, "SKIP");
                    return false;
                }

                var watch = Stopwatch.StartNew();
                target.AsRadioButton().IsChecked = true;
                bool selected = WaitSelected(() => FindSidebar(_session.WaitForMainWindow(), id),
                    e => e.AsRadioButton().IsChecked, TimeSpan.FromSeconds(8));
                if (!selected) throw new TimeoutException("Sidebar not selected: " + id);
                watch.Stop();
                // The selection measurement excludes UIA tree inspection and
                // the deliberate animation-settle delay.
                _session.Record("navigation.sidebar", id, watch.ElapsedMilliseconds, "PASS");
                Stabilize(mode);
                AssertVisibleWindow(mode == NavigationMode.Normal);
                if (mode == NavigationMode.Normal)
                    _session.CaptureState("sidebar-" + id, true);
                return true;
            }
            catch (Exception ex)
            {
                failures.Add(id + ": " + ex.Message);
                _session.Record("navigation.sidebar", id, 0, "FAIL." + ex.GetType().Name);
                return false;
            }
        }

        private void TraverseTabGroups(NavigationMode mode, HashSet<string> visitedGroups,
            List<string> failures, ref int tabs, ref int actions, int depth)
        {
            if (depth >= 4 || actions >= MaxActions) return;

            foreach (string groupId in TabGroupIds(_session.WaitForMainWindow()))
            {
                if (!visitedGroups.Add(groupId)) continue;
                _session.Record("coverage.group", groupId, 0, "DISCOVERED");
                var ids = TabIds(groupId);
                if (ids.Length == 0)
                {
                    _session.Record("coverage.skip", groupId, 0, "SKIP");
                    continue;
                }

                foreach (string id in Order(ids, mode))
                {
                    if (actions >= MaxActions) break;
                    try
                    {
                        var tab = FindTab(groupId, id);
                        if (tab == null || !Visible(tab))
                        {
                            _session.Record("coverage.skip", groupId + "." + id, 0, "SKIP");
                            continue;
                        }

                        var watch = Stopwatch.StartNew();
                        tab.AsTabItem().Select();
                        bool selected = WaitSelected(() => FindTab(groupId, id),
                            e => e.AsTabItem().IsSelected, TimeSpan.FromSeconds(8));
                        if (!selected) throw new TimeoutException("Tab not selected: " + groupId + "." + id);
                        watch.Stop();
                        _session.Record("navigation.tab", groupId + "." + id,
                            watch.ElapsedMilliseconds, "PASS");
                        Stabilize(mode);
                        AssertVisibleWindow(mode == NavigationMode.Normal);
                        tabs++;
                        actions++;
                        if (mode == NavigationMode.Normal)
                            _session.CaptureState("tab-" + groupId + "-" + id, true);

                        // Newly revealed nested tab groups receive their own pass.
                        TraverseTabGroups(mode, visitedGroups, failures, ref tabs, ref actions, depth + 1);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(groupId + "/" + id + ": " + ex.Message);
                        _session.Record("navigation.tab", groupId + "." + id, 0,
                            "FAIL." + ex.GetType().Name);
                    }
                }
            }
        }

        private static IEnumerable<string> Order(string[] values, NavigationMode mode)
        {
            if (mode == NavigationMode.Reverse) return values.Reverse();
            if (mode != NavigationMode.Shuffle) return values;

            // Stable seed, so failures are reproducible.
            var random = new Random(170728);
            return values.OrderBy(_ => random.Next()).ToArray();
        }

        private static void Stabilize(NavigationMode mode)
        {
            // Let the 260 ms indicator and 380 ms page transition reach
            // their final layout BEFORE structural checks / screenshots.
            Thread.Sleep(mode == NavigationMode.Normal ? 420 : 80);
        }

        private static bool WaitSelected(Func<AutomationElement> find,
            Func<AutomationElement, bool> predicate, TimeSpan timeout)
        {
            return Retry.WhileNull(() =>
            {
                var element = find();
                return element != null && Visible(element) && predicate(element) ? element : null;
            }, timeout: timeout, interval: TimeSpan.FromMilliseconds(75),
                ignoreException: true).Result != null;
        }

        private void AssertVisibleWindow(bool inspectAccessibility = false)
        {
            var window = _session.WaitForMainWindow();
            var bounds = window.BoundingRectangle;
            Assert.That(bounds.Width, Is.GreaterThan(500), "Window width collapsed.");
            Assert.That(bounds.Height, Is.GreaterThan(300), "Window height collapsed.");
            Assert.That(window.IsOffscreen, Is.False, "Main window became offscreen.");
            // Full UIA traversal is expensive and can itself delay the
            // Dispatcher. Run it only in the standard (non-stress) scenario.
            if (!inspectAccessibility) return;

            // Structural checks are not pixel-perfect proof of layout quality.
            var descendants = window.FindAllDescendants();
            Assert.That(descendants.Count(Visible), Is.GreaterThan(5),
                "New page has almost no accessible visuals.");

            var hosts = descendants.Where(e => e.AutomationId != null &&
                e.AutomationId.StartsWith("qa.content.", StringComparison.Ordinal)).ToArray();
            if (hosts.Length == 0)
            {
                _session.Record("render.content", "uia-hosts", 0, "UNVERIFIED.no-peers");
                return;
            }

            Assert.That(hosts.Any(h => Visible(h) &&
                h.BoundingRectangle.Width > 40 && h.BoundingRectangle.Height > 40),
                Is.True, "Content host collapsed or is no longer visible.");
            _session.Record("render.content", "uia-hosts", 0, "PASS");
        }

        private static string[] SidebarIds(Window window)
        {
            return window.FindAllDescendants(cf => cf.ByControlType(ControlType.RadioButton))
                .Where(Visible).Select(e => e.AutomationId)
                .Where(s => s.StartsWith("nav.sidebar.", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal).ToArray();
        }

        private string[] TabGroupIds(Window window)
        {
            return window.FindAllDescendants(cf => cf.ByControlType(ControlType.Tab))
                .Where(Visible).Select(e => e.AutomationId)
                .Where(s => s.StartsWith("nav.group.", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal).ToArray();
        }

        private string[] TabIds(string groupId)
        {
            var group = FindTabGroup(groupId);
            if (group == null) return new string[0];
            return group.FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem))
                .Where(Visible).Select(e => e.AutomationId)
                .Where(s => s.StartsWith("nav.tab.", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal).ToArray();
        }

        private AutomationElement FindTabGroup(string id)
        {
            return _session.WaitForMainWindow().FindFirstDescendant(cf =>
                cf.ByAutomationId(id).And(cf.ByControlType(ControlType.Tab)));
        }

        private AutomationElement FindSidebar(Window window, string id)
        {
            return window.FindFirstDescendant(cf => cf.ByAutomationId(id)
                .And(cf.ByControlType(ControlType.RadioButton)));
        }

        private AutomationElement FindTab(string groupId, string tabId)
        {
            var group = FindTabGroup(groupId);
            return group?.FindFirstDescendant(cf => cf.ByAutomationId(tabId)
                .And(cf.ByControlType(ControlType.TabItem)));
        }

        private static bool Visible(AutomationElement element)
        {
            try
            {
                return element.IsEnabled && !element.IsOffscreen &&
                    element.BoundingRectangle.Width > 1 && element.BoundingRectangle.Height > 1;
            }
            catch (Exception)
            {
                // WPF can destroy/recreate the UIA peer during a transition.
                return false;
            }
        }
    }
}
