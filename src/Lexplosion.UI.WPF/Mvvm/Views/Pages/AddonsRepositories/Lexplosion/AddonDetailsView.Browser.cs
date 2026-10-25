using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories;
using Lexplosion.UI.WPF.Services.Addons;
using Lexplosion.UI.WPF.Services.Descriptions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private void ShowLoading(string text)
        {
            LoadingText.Text = text;
            LoadingBar.Visibility = Visibility.Visible;
            LoadingOverlay.Visibility = Visibility.Visible;
            if (_browser != null) _browser.Visibility = Visibility.Hidden;
        }

        private void ShowError(string text)
        {
            LoadingText.Text = text;
            LoadingBar.Visibility = Visibility.Collapsed;
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
            await browser.EnsureCoreWebView2Async();
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
            browser.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
            browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        }

        private async Task RenderAddonAsync(InstanceAddon addon, int generation)
        {
            if (addon == null || !IsLoaded || !IsVisible) return;
            ShowLoading(L("AddonDetailsDescriptionLoading"));
            try
            {
                Task<AddonPageData> dataTask = Task.Run(() => AddonDetailsDataLoader.FetchPage(addon));
                await EnsureBrowserAsync();
                AddonPageData data = await dataTask;
                if (generation != _generation || !IsLoaded || !IsVisible || !ReferenceEquals(Addon, addon)) return;
                GalleryItems.ItemsSource = data.Images;
                // Modrinth sends game versions oldest-first. Present the recent
                // stable releases first rather than a wall of old snapshots.
                var stableVersions = data.MinecraftVersions.Where(version =>
                    Regex.IsMatch(version ?? string.Empty, @"^\d+\.\d+(?:\.\d+)?$")).ToArray();
                MinecraftVersionsItems.ItemsSource = Labels(
                    Enumerable.Reverse(stableVersions.Length > 0 ? stableVersions : data.MinecraftVersions), 7);
                ModloadersItems.ItemsSource = Labels(data.Modloaders, 6);
                GalleryEmptyText.Text = data.Images.Length == 0
                    ? L("AddonDetailsGalleryEmpty") : string.Empty;
                GalleryEmptyText.Visibility = data.Images.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

                var document = AddonDescriptionDocumentBuilder.Build(data.Description, addon.Source, ReadTheme(), L("AddonDetailsNoDescription"));
                string filename = "addon-" + Guid.NewGuid().ToString("N") + ".html";
                string fullPath = Path.Combine(HtmlFolder, filename);
                File.WriteAllText(fullPath, document, new UTF8Encoding(false));
                _temporaryDocuments.Add(fullPath);
                _pendingDocumentGeneration = generation;
                _browser.CoreWebView2.Navigate("https://" + VirtualHost + "/" + filename);
            }
            catch (Exception ex)
            {
                if (generation == _generation && IsLoaded && IsVisible)
                    ShowError(string.Format(L("AddonDetailsDescriptionError"), ex.Message));
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

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.NavigationId != _currentNavigationId ||
                _pendingDocumentGeneration != _generation) return;
            if (!IsLoaded || !IsVisible || Addon == null) return;
            if (!e.IsSuccess)
            {
                if (e.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
                    ShowError(string.Format(L("AddonDetailsWebViewError"), e.WebErrorStatus));
                return;
            }
            if (_browser != null)
            {
                _browser.Visibility = DetailsTabs.SelectedIndex == 0
                    ? Visibility.Visible : Visibility.Hidden;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            OpenExternalUrl(e.Uri);
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (e.TryGetWebMessageAsString() == "lexplosion-overview-escape")
                (DataContext as LexplosionAddonsRepositoryViewModel)?.CloseAddonDetails();
        }

        private static void OpenExternalUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)) return;
            try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
            catch (Exception) { /* Opening external links is optional. */ }
        }

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
