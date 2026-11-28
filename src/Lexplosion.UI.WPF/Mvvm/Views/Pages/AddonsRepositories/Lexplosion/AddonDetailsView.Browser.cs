using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories;
using Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories;
using Lexplosion.UI.WPF.Services.Addons;
using Lexplosion.UI.WPF.Services.Descriptions;
#if DEBUG
using Lexplosion.UI.WPF.Core;
#endif
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.AddonDetails
{
    public partial class AddonDetailsView
    {
        private const string VirtualHost = "lexplosion-addon.appassets.example";
        private static readonly string HtmlFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lexplosion", "Cache", "AddonDetailsWebView2");

        private WebView2 _browser;
        private Task _initializationTask;
        private readonly List<string> _temporaryDocuments = new List<string>();
        private ulong _currentNavigationId;
        private int _pendingDocumentGeneration = -1;
        private int _generation;
        private AddonDetailsModel _model;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _model = (DataContext as AddonDetailsViewModel)?.Model;
            if (_model == null)
            {
                return;
            }

            _model.PropertyChanged += OnModelPropertyChanged;
            _ = RenderAddonAsync(_model, ++_generation);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _generation++;
            if (_model != null)
            {
                _model.PropertyChanged -= OnModelPropertyChanged;
                _model = null;
            }

            CleanupBrowser();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AddonDetailsModel.IsInformationOpen) || e.PropertyName == nameof(AddonDetailsModel.SelectedTabIndex))
            {
                UpdateBrowserHitTesting();
            }
        }

        private void UpdateBrowserHitTesting()
        {
            if (_browser == null)
            {
                return;
            }

            _browser.Visibility = _model != null && !_model.IsInformationOpen && _model.SelectedTabIndex == 0 &&
                LoadingOverlay.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Hidden;
        }

        private void OnInformationScrimClick(object sender, MouseButtonEventArgs e)
        {
            _model?.CloseInformation();
            e.Handled = true;
        }

        private void CleanupBrowser()
        {
            if (_browser != null)
            {
                try
                {
                    if (_browser.CoreWebView2 != null)
                    {
                        _browser.CoreWebView2.NavigationStarting -= OnNavigationStarting;
                        _browser.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
                        _browser.CoreWebView2.DOMContentLoaded -= OnDomContentLoaded;
                        _browser.CoreWebView2.NewWindowRequested -= OnNewWindowRequested;
                        _browser.CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
                    }
                    BrowserHost.Children.Remove(_browser);
                    _browser.Dispose();
                }
                catch (Exception) { /* The browser may already be shutting down. */ }
                _browser = null;
                _initializationTask = null;
            }
            foreach (string file in _temporaryDocuments)
            {
                try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
            _temporaryDocuments.Clear();
        }

        private void ShowLoading()
        {
            LoadingText.SetResourceReference(TextBlock.TextProperty, "AddonDetailsDescriptionLoading");
            LoadingBar.IsActive = true;
            LoadingOverlay.Visibility = Visibility.Visible;
            if (_browser != null) _browser.Visibility = Visibility.Hidden;
        }

        private void ShowError(string text)
        {
            LoadingText.Text = text;
            LoadingBar.IsActive = false;
            LoadingOverlay.Visibility = Visibility.Visible;
            if (_browser != null) _browser.Visibility = Visibility.Hidden;
        }

        private Task EnsureBrowserAsync()
        {
            if (_initializationTask == null)
                _initializationTask = InitializeBrowserAsync();
            return _initializationTask;
        }

        private async Task InitializeBrowserAsync()
        {
            Directory.CreateDirectory(HtmlFolder);
            var browser = new WebView2
            {
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                DefaultBackgroundColor = ToDrawingColor("PageSolidColorBrush")
            };
            _browser = browser;
            BrowserHost.Children.Add(browser);
#if DEBUG
            long browserInitTick = Stopwatch.GetTimestamp();
#endif
            await browser.EnsureCoreWebView2Async();
#if DEBUG
            RecordElapsed("addon.webview.initialize", browserInitTick);
#endif
            if (!IsLoaded || !ReferenceEquals(browser, _browser)) return;

            var settings = browser.CoreWebView2.Settings;
            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDevToolsEnabled = false;
            settings.IsStatusBarEnabled = false;
            settings.IsWebMessageEnabled = true;
            browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                VirtualHost, HtmlFolder, CoreWebView2HostResourceAccessKind.DenyCors);
            browser.CoreWebView2.NavigationStarting += OnNavigationStarting;
            browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            browser.CoreWebView2.DOMContentLoaded += OnDomContentLoaded;
            browser.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
            browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        }

        private async Task RenderAddonAsync(AddonDetailsModel model, int generation)
        {
            ShowLoading();
            try
            {
                await Task.WhenAll(model.LoadPageAsync(), EnsureBrowserAsync());
                if (generation != _generation || !IsLoaded || !ReferenceEquals(model, _model))
                {
                    return;
                }

                var data = model.PageData;
                var theme = ReadTheme();
                string noDescription = ResourceText("AddonDetailsNoDescription");
                string filename = "addon-" + Guid.NewGuid().ToString("N") + ".html";
                string fullPath = Path.Combine(HtmlFolder, filename);
                await Task.Run(() =>
                {
#if DEBUG
                    using var trace = UiDebugTrace.Measure("addon.html.build-write", 5);
#endif
                    string document = AddonDescriptionDocumentBuilder.Build(data.Description, model.Addon.Source, theme, noDescription);
                    File.WriteAllText(fullPath, document, new UTF8Encoding(false));
                });

                if (generation != _generation || !IsLoaded || !ReferenceEquals(model, _model))
                {
                    try { File.Delete(fullPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                    return;
                }

                _temporaryDocuments.Add(fullPath);
                _pendingDocumentGeneration = generation;
#if DEBUG
                using (UiDebugTrace.Measure("addon.webview.navigate", 5))
#endif
                    _browser.CoreWebView2.Navigate("https://" + VirtualHost + "/" + filename);
            }
            catch (Exception ex)
            {
                if (generation == _generation && IsLoaded && ReferenceEquals(model, _model))
                {
                    ShowError(string.Format(ResourceText("AddonDetailsDescriptionError"), ex.Message));
                }
            }
        }

        private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) &&
                uri.Scheme == Uri.UriSchemeHttps &&
                uri.Host.Equals(VirtualHost, StringComparison.OrdinalIgnoreCase))
            {
                _currentNavigationId = e.NavigationId;
                return;
            }
            e.Cancel = true;
            OpenExternalUrl(e.Uri);
        }

        private void OnDomContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            RevealDocument(e.NavigationId);
        }

        private void RevealDocument(ulong navigationId)
        {
            if (navigationId != _currentNavigationId || _pendingDocumentGeneration != _generation || !IsLoaded || _model == null)
            {
                return;
            }

            LoadingBar.IsActive = false;
            LoadingOverlay.Visibility = Visibility.Collapsed;
            UpdateBrowserHitTesting();
        }

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
#if DEBUG
            using var trace = UiDebugTrace.Measure("addon.webview.completed", 3);
#endif
            if (e.NavigationId != _currentNavigationId ||
                _pendingDocumentGeneration != _generation) return;
            if (!IsLoaded || _model == null) return;
            if (!e.IsSuccess)
            {
                if (e.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
                    ShowError(string.Format(ResourceText("AddonDetailsWebViewError"), e.WebErrorStatus));
                return;
            }
            RevealDocument(e.NavigationId);
        }

        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            OpenExternalUrl(e.Uri);
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (e.TryGetWebMessageAsString() == "lexplosion-overview-escape")
            {
                (DataContext as AddonDetailsViewModel)?.CloseAddonDetailsCommand.Execute(null);
            }
        }

#if DEBUG
        private static void RecordElapsed(string operation, long start)
        {
            Lexplosion.QA.PerformanceTelemetry.Record(operation,
                (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency, 5);
        }
#endif

        private static void OpenExternalUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)) return;
            try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
            catch (Exception) { /* Opening external links is optional. */ }
        }

        private string ResourceText(string key) => TryFindResource(key) as string ?? key;

        private string BrushHex(string key, string fallback)
        {
            var brush = TryFindResource(key) as SolidColorBrush;
            return brush == null ? fallback :
                "#" + brush.Color.R.ToString("X2") + brush.Color.G.ToString("X2") + brush.Color.B.ToString("X2");
        }

        private System.Drawing.Color ToDrawingColor(string key)
        {
            var brush = TryFindResource(key) as SolidColorBrush;
            return brush == null ? System.Drawing.Color.FromArgb(11, 16, 32) :
                System.Drawing.Color.FromArgb(brush.Color.R, brush.Color.G, brush.Color.B);
        }

        private InstanceDescriptionTheme ReadTheme()
        {
            return new InstanceDescriptionTheme
            {
                Background = BrushHex("PageSolidColorBrush", "#0B1020"),
                Foreground = BrushHex("PrimaryForegroundSolidColorBrush", "#E8EBF2"),
                Muted = BrushHex("SecondaryForegroundSolidColorBrush", "#A9B7CA"),
                Accent = BrushHex("ActivitySolidColorBrush", "#1987FF"),
                Surface = BrushHex("PrimarySolidColorBrush", "#10192A"),
                Border = BrushHex("SeparateSolidColorBrush", "#243149")
            };
        }
    }
}
