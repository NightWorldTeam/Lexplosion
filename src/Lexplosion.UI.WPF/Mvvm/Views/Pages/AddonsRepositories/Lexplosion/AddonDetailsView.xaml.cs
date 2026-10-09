using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories;
using Lexplosion.UI.WPF.Services.Descriptions;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.AddonDetails
{
    /// <summary>
    /// Project page for catalog addons. The existing repository owns installation,
    /// filters and download state; this view only loads rich project information.
    /// </summary>
    public partial class AddonDetailsView : UserControl
    {
        private const string VirtualHost = "lexplosion-addon.appassets.example";
        private static readonly string HtmlFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lexplosion", "Cache", "AddonDetailsWebView2");

        public static readonly DependencyProperty AddonProperty = DependencyProperty.Register(
            nameof(Addon), typeof(InstanceAddon), typeof(AddonDetailsView),
            new PropertyMetadata(null, OnAddonPropertyChanged));

        public InstanceAddon Addon
        {
            get => (InstanceAddon)GetValue(AddonProperty);
            set => SetValue(AddonProperty, value);
        }

        // WPF layout is measured in DIPs (ActualWidth), so typography follows
        // launcher window size rather than the physical monitor pixel count.
        public static readonly DependencyProperty InfoLabelFontSizeProperty = DependencyProperty.Register(
            nameof(InfoLabelFontSize), typeof(double), typeof(AddonDetailsView), new PropertyMetadata(12.0));
        public static readonly DependencyProperty InfoValueFontSizeProperty = DependencyProperty.Register(
            nameof(InfoValueFontSize), typeof(double), typeof(AddonDetailsView), new PropertyMetadata(14.0));
        public static readonly DependencyProperty InfoChipFontSizeProperty = DependencyProperty.Register(
            nameof(InfoChipFontSize), typeof(double), typeof(AddonDetailsView), new PropertyMetadata(12.0));
        public static readonly DependencyProperty InfoActionFontSizeProperty = DependencyProperty.Register(
            nameof(InfoActionFontSize), typeof(double), typeof(AddonDetailsView), new PropertyMetadata(13.0));
        public static readonly DependencyProperty InfoTitleFontSizeProperty = DependencyProperty.Register(
            nameof(InfoTitleFontSize), typeof(double), typeof(AddonDetailsView), new PropertyMetadata(17.0));
        public static readonly DependencyProperty TabFontSizeProperty = DependencyProperty.Register(
            nameof(TabFontSize), typeof(double), typeof(AddonDetailsView), new PropertyMetadata(13.0));
        public static readonly DependencyProperty InfoChipPaddingProperty = DependencyProperty.Register(
            nameof(InfoChipPadding), typeof(Thickness), typeof(AddonDetailsView),
            new PropertyMetadata(new Thickness(9, 5, 9, 5)));

        public double InfoLabelFontSize { get => (double)GetValue(InfoLabelFontSizeProperty); private set => SetValue(InfoLabelFontSizeProperty, value); }
        public double InfoValueFontSize { get => (double)GetValue(InfoValueFontSizeProperty); private set => SetValue(InfoValueFontSizeProperty, value); }
        public double InfoChipFontSize { get => (double)GetValue(InfoChipFontSizeProperty); private set => SetValue(InfoChipFontSizeProperty, value); }
        public double InfoActionFontSize { get => (double)GetValue(InfoActionFontSizeProperty); private set => SetValue(InfoActionFontSizeProperty, value); }
        public double InfoTitleFontSize { get => (double)GetValue(InfoTitleFontSizeProperty); private set => SetValue(InfoTitleFontSizeProperty, value); }
        public double TabFontSize { get => (double)GetValue(TabFontSizeProperty); private set => SetValue(TabFontSizeProperty, value); }
        public Thickness InfoChipPadding { get => (Thickness)GetValue(InfoChipPaddingProperty); private set => SetValue(InfoChipPaddingProperty, value); }

        private WebView2 _browser;
        private Task _initializationTask;
        private readonly List<string> _temporaryDocuments = new List<string>();
        private bool _expandedInformation;
        private int _generation;
        private int _versionsLoadedForGeneration = -1;
        private ulong _currentNavigationId;
        private int _pendingDocumentGeneration = -1;

        private sealed class AddonPageData
        {
            public string Description { get; set; }
            public string[] Images { get; set; } = Array.Empty<string>();
            public string[] MinecraftVersions { get; set; } = Array.Empty<string>();
            public string[] Modloaders { get; set; } = Array.Empty<string>();
        }

        public AddonDetailsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnVisibilityChanged;
        }

        private static void OnAddonPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var view = (AddonDetailsView)sender;
            view._generation++;
            view._versionsLoadedForGeneration = -1;
            view.VersionsList.ItemsSource = null;
            view.InstallVersionButton.IsEnabled = false;
            view.VersionsStatus.Text = "Выберите вкладку для загрузки версий.";
            view.GalleryItems.ItemsSource = null;
            // Show only grounded metadata. The full Modrinth response can fill these later.
            view.MinecraftVersionsItems.ItemsSource = Labels(
                string.IsNullOrWhiteSpace(view.Addon?.GameVersion)
                    ? Array.Empty<string>() : new[] { view.Addon.GameVersion }, 8);
            view.ModloadersItems.ItemsSource = Labels(view.Addon?.DisplayModloaders, 6);
            view.GalleryEmptyText.Text = "Загрузка галереи…";
            view.GalleryEmptyText.Visibility = Visibility.Visible;
            view.CategoryItems.ItemsSource = Labels(
                view.Addon?.Categories?.Select(category => category.Name), 10);
            view.ProjectWebsiteLabel.Text = GetResourceCaption(view.Addon);
            view.UpdateResourcePresentation();
            view.UpdateInformationLayout();
            view.DetailsTabs.SelectedIndex = 0;
            if (view.IsLoaded && view.IsVisible && view.Addon != null)
                _ = view.RenderAddonAsync(view.Addon, view._generation);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateInformationLayout();
            if (Addon != null && IsVisible)
                _ = RenderAddonAsync(Addon, ++_generation);
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                ++_generation; // Ignore responses for the previously opened project.
                if (_browser != null) _browser.Visibility = Visibility.Hidden;
                return;
            }
            if (IsLoaded && Addon != null)
                _ = RenderAddonAsync(Addon, ++_generation);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ++_generation;
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

        private static string[] Labels(IEnumerable<string> values, int maxItems)
        {
            var list = (values ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (list.Length == 0) return new[] { "—" };
            if (list.Length <= maxItems) return list;
            return list.Take(maxItems).Concat(new[] { "+" + (list.Length - maxItems) }).ToArray();
        }

        private static AddonPageData FetchPage(InstanceAddon addon)
        {
            var data = new AddonPageData
            {
                Description = addon.Description ?? string.Empty,
                MinecraftVersions = string.IsNullOrWhiteSpace(addon.GameVersion)
                    ? Array.Empty<string>() : new[] { addon.GameVersion },
                Modloaders = addon.DisplayModloaders?.ToArray() ?? Array.Empty<string>()
            };
            if (addon.Source == ProjectSource.Modrinth)
            {
                // InstanceAddon already loads WebsiteUrl on its worker thread.
                // A catalog card may be opened before that background work completes.
                string url = addon.WebsiteUrl;
                for (int attempt = 0; attempt < 30 && string.IsNullOrWhiteSpace(url); attempt++)
                {
                    Thread.Sleep(150);
                    url = addon.WebsiteUrl;
                }
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                    (uri.Host.Equals("modrinth.com", StringComparison.OrdinalIgnoreCase) ||
                     uri.Host.Equals("www.modrinth.com", StringComparison.OrdinalIgnoreCase)))
                {
                    string slug = uri.AbsolutePath.Trim('/').Split('/').LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(slug))
                    {
                        ModrinthProjectInfo project = null;
                        try { project = Runtime.ServicesContainer.MdApi.GetProject(slug); }
                        catch (Exception) { /* Show the catalog summary if the API is offline. */ }
                        if (project != null)
                        {
                            data.Description = string.IsNullOrWhiteSpace(project.FullDescription)
                                ? data.Description : project.FullDescription;
                            if (project.GameVersions?.Count > 0)
                                data.MinecraftVersions = project.GameVersions.ToArray();
                            if (project.Loaders?.Count > 0)
                                data.Modloaders = project.Loaders.ToArray();
                            data.Images = project.Images == null ? Array.Empty<string>() :
                                project.Images.Select(item => item != null && item.TryGetValue("url", out var image)
                                    ? image : null)
                                .Where(image => Uri.TryCreate(image, UriKind.Absolute, out var parsed) &&
                                    parsed.Scheme == Uri.UriSchemeHttps)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToArray();
                        }
                    }
                }
            }
            else if (addon.Source == ProjectSource.Curseforge)
            {
                try
                {
                    string description = addon.GetFullDescription();
                    if (!string.IsNullOrWhiteSpace(description)) data.Description = description;
                }
                catch (Exception) { /* Fall back to the short catalog description. */ }
            }
            return data;
        }

        private async Task RenderAddonAsync(InstanceAddon addon, int generation)
        {
            if (addon == null || !IsLoaded || !IsVisible) return;
            ShowLoading("Загрузка описания…");
            try
            {
                Task<AddonPageData> dataTask = Task.Run(() => FetchPage(addon));
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
                    ? "У проекта нет доступных изображений в каталоге." : string.Empty;
                GalleryEmptyText.Visibility = data.Images.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

                InstanceSource source = addon.Source == ProjectSource.Curseforge
                    ? InstanceSource.Curseforge : InstanceSource.Modrinth;
                InstanceDescriptionTheme theme = ReadTheme();
                // The shared renderer is used by instance profiles too. Customize only
                // this document; don't change instance pages or their WebView behavior.
                string document = InstanceDescriptionHtmlBuilder.Build(
                    data.Description, source, theme.Background, null, theme);
                // The builder unconditionally adds an "Информация о сборке" HTML aside.
                // The addon page already has a native WPF facts panel, so remove ONLY
                // that generated aside. The HTML remains sanitized by the builder.
                document = Regex.Replace(document,
                    @"<aside class='metadata-sidebar'[^>]*>.*?</aside>",
                    string.Empty, RegexOptions.Singleline);
                const string layoutCss = @"
                    main.overview-layout{display:block!important;width:100%!important;
                        max-width:none!important;margin:0!important;gap:0!important;}
                    article.description-content{width:100%!important;max-width:1140px!important;
                        margin:0 auto!important;padding:0!important;text-align:left!important;}
                    /* Match the 24px tab-strip inset and keep readable line lengths
                       on maximized windows without hugging the far left edge. */
                    body{padding:28px 34px 68px!important;
                        font-size:clamp(14px,0.33vw + 11px,18px)!important;line-height:1.70!important;}
                    article.description-content > :first-child{margin-top:0!important;}
                    article.description-content > h1:first-child{font-size:clamp(25px,0.35vw + 22px,29px)!important;}
                    article.description-content p{margin-top:.55em;margin-bottom:.95em;}
                    article.description-content li{margin:5px 0;}
                    article.description-content h2{margin-top:1.7em;}
                    article.description-content img{height:auto;max-width:100%;}
                    @media (max-width:1180px){
                        article.description-content{max-width:100%!important;margin:0!important;}
                    }
                    @media (max-width:760px){
                        body{padding:22px 24px 48px!important;}
                        article.description-content > h1:first-child{font-size:23px!important;}
                    }";
                document = document.Replace("</style></head>", layoutCss + "</style></head>");
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
                    ShowError("Не удалось загрузить описание: " + ex.Message);
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
                    ShowError("Не удалось отобразить описание: " + e.WebErrorStatus);
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

        private async void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, DetailsTabs)) return;
            // A native HWND must not cover the WPF gallery or version list.
            if (_browser != null)
                _browser.Visibility = DetailsTabs.SelectedIndex == 0 &&
                    LoadingOverlay.Visibility == Visibility.Collapsed
                    ? Visibility.Visible : Visibility.Hidden;
            if (DetailsTabs.SelectedIndex != 2) return;
            var addon = Addon;
            int generation = _generation;
            if (addon == null || _versionsLoadedForGeneration == generation) return;
            _versionsLoadedForGeneration = generation;
            VersionsStatus.Text = "Загрузка версий…";
            try
            {
                var versions = await Task.Run(() => addon.GetAllVersion()?.Values.ToArray() ?? Array.Empty<object>());
                if (generation != _generation || !ReferenceEquals(addon, Addon)) return;
                VersionsList.ItemsSource = versions;
                if (versions.Length > 0) VersionsList.SelectedIndex = 0;
                InstallVersionButton.IsEnabled = VersionsList.SelectedItem != null;
                VersionsStatus.Text = versions.Length == 0 ? "Для этой версии Minecraft файлы не найдены."
                    : "Выберите версию и нажмите кнопку установки.";
            }
            catch (Exception ex)
            {
                if (generation == _generation)
                {
                    _versionsLoadedForGeneration = -1;
                    VersionsStatus.Text = "Ошибка загрузки версий: " + ex.Message;
                }
            }
        }

        private void OnVersionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstallVersionButton != null)
                InstallVersionButton.IsEnabled = VersionsList.SelectedItem != null;
        }

        private void OnInstallVersionClick(object sender, RoutedEventArgs e)
        {
            if (VersionsList.SelectedItem != null)
                (DataContext as LexplosionAddonsRepositoryViewModel)?.InstallSelectedAddonVersion(VersionsList.SelectedItem);
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            (DataContext as LexplosionAddonsRepositoryViewModel)?.CloseAddonDetails();
        }

        private static string GetResourceCaption(InstanceAddon addon)
        {
            if (addon?.Source == ProjectSource.Modrinth) return "Перейти на Modrinth";
            if (addon?.Source == ProjectSource.Curseforge) return "Перейти на CurseForge";
            return "Перейти на ресурс";
        }

        private void UpdateResourcePresentation()
        {
            bool isModrinth = Addon?.Source == ProjectSource.Modrinth;
            bool isCurseforge = Addon?.Source == ProjectSource.Curseforge;
            ProjectSourceLabel.Text = isModrinth ? "Modrinth" : isCurseforge ? "CurseForge" : "Ресурс";
            ProjectModrinthIcon.Visibility = isModrinth ? Visibility.Visible : Visibility.Collapsed;
            ProjectCurseforgeIcon.Visibility = isCurseforge ? Visibility.Visible : Visibility.Collapsed;
            ProjectOtherIcon.Visibility = !isModrinth && !isCurseforge
                ? Visibility.Visible : Visibility.Collapsed;
        }


        private void OnOpenWebsiteClick(object sender, RoutedEventArgs e)
        {
            string url = Addon?.WebsiteUrl;
            // Older Core models generate /modpack/{slug} for every Modrinth
            // project. Preserve their URL for other types but repair mod links.
            if (Addon?.Source == ProjectSource.Modrinth && Addon.Type == AddonType.Mods &&
                Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Host.Equals("modrinth.com", StringComparison.OrdinalIgnoreCase) ||
                 uri.Host.Equals("www.modrinth.com", StringComparison.OrdinalIgnoreCase)))
            {
                string slug = uri.AbsolutePath.Trim('/').Split('/').LastOrDefault();
                if (!string.IsNullOrWhiteSpace(slug))
                    url = "https://modrinth.com/mod/" + Uri.EscapeDataString(slug);
            }
            OpenExternalUrl(url);
        }


        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (Addon == null) return;
            var viewModel = DataContext as LexplosionAddonsRepositoryViewModel;
            if (viewModel?.UninstallAddonCommand?.CanExecute(Addon) == true)
                viewModel.UninstallAddonCommand.Execute(Addon);
        }

        private void OnInformationScrimClick(object sender, RoutedEventArgs e)
        {
            if (_expandedInformation)
            {
                _expandedInformation = false;
                UpdateInformationLayout();
            }
        }
        private void OnInformationClick(object sender, RoutedEventArgs e)
        {
            _expandedInformation = !_expandedInformation;
            UpdateInformationLayout();
        }

        private void OnDetailSizeChanged(object sender, SizeChangedEventArgs e) => UpdateInformationLayout();

        private void UpdateInformationLayout()
        {
            if (InformationColumn == null) return;
            double available = DetailRoot.ActualWidth;
            bool wide = available >= 1340;
            bool extraWide = available >= 1700;
            bool show = wide || _expandedInformation;
            double width = wide ? (extraWide ? 382 : 348) : Math.Min(332, Math.Max(248, available * 0.41));
            InformationColumn.Width = show ? new GridLength(width) : new GridLength(0);
            InformationPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            InformationButton.Visibility = wide ? Visibility.Collapsed : Visibility.Visible;
            CloseInformationButton.Visibility = wide ? Visibility.Collapsed : Visibility.Visible;
            InformationScrim.Visibility = !wide && show ? Visibility.Visible : Visibility.Collapsed;
            InformationScrim.IsHitTestVisible = !wide && show;

            bool compact = available < 1120;
            // Match Lexplosion library's SFProDisplay type scale: at the default
            // 944px window the details are readable; on a full-size window ALL
            // sidebar labels, values, chips, tabs and actions scale together.
            bool largeText = available >= 1480;
            bool veryLargeText = available >= 1880;
            InfoLabelFontSize = veryLargeText ? 15.0 : largeText ? 13.5 : compact ? 12.0 : 12.5;
            InfoValueFontSize = veryLargeText ? 17.0 : largeText ? 15.5 : compact ? 14.0 : 14.5;
            InfoChipFontSize = veryLargeText ? 15.0 : largeText ? 13.5 : compact ? 12.0 : 12.5;
            InfoActionFontSize = veryLargeText ? 16.0 : largeText ? 15.0 : compact ? 13.0 : 14.0;
            InfoTitleFontSize = veryLargeText ? 22.0 : largeText ? 20.0 : 17.0;
            TabFontSize = veryLargeText ? 16.0 : largeText ? 15.0 : compact ? 13.0 : 14.0;
            InfoChipPadding = veryLargeText ? new Thickness(12, 7, 12, 7)
                : largeText ? new Thickness(11, 6, 11, 6) : new Thickness(9, 5, 9, 5);
            double sourceIconSize = veryLargeText ? 24 : largeText ? 22 : 20;
            ProjectModrinthIcon.Width = ProjectModrinthIcon.Height = sourceIconSize;
            ProjectCurseforgeIcon.Width = ProjectCurseforgeIcon.Height = sourceIconSize;
            ProjectOtherIcon.Width = ProjectOtherIcon.Height = sourceIconSize;
            HeaderSurface.Padding = compact ? new Thickness(18, 10, 18, 11)
                : new Thickness(24, 12, 24, 12);
            AddonLogo.Width = AddonLogo.Height = compact ? 54 : 64;
            AddonLogo.Margin = compact ? new Thickness(12, 0, 14, 0)
                : new Thickness(14, 0, 16, 0);
            AddonTitle.FontSize = compact ? 19 : extraWide ? 24 : 22;
            AddonSummary.FontSize = compact ? 12 : extraWide ? 13 : 12;
            AddonSummary.Visibility = Visibility.Visible;
            HeaderInstallButton.MinWidth = compact ? 110 : 138;
            HeaderInstallButton.Padding = compact ? new Thickness(12, 0, 12, 0)
                : new Thickness(18, 0, 18, 0);
            HeaderDeleteButton.Height = compact ? 34 : 36;
            HeaderDeleteButton.MinWidth = compact ? 36 : 98;
            BackButton.Width = BackButton.Height = compact ? 36 : 38;
            BackButton.Margin = new Thickness(0, compact ? 1 : 0, 0, 0);
            if (DetailsTabs.Template != null)
            {
                var headerPanel = DetailsTabs.Template.FindName("HeaderPanel", DetailsTabs) as System.Windows.Controls.Primitives.TabPanel;
                if (headerPanel != null)
                    headerPanel.Margin = new Thickness(
                        HeaderSurface.Padding.Left + BackButton.Width + AddonLogo.Margin.Left,
                        0, 0, 0);
            }
            InformationButton.Width = InformationButton.Height = compact ? 34 : 38;
            InformationStack.Margin = veryLargeText ? new Thickness(30, 10, 26, 36)
                : largeText ? new Thickness(26, 8, 24, 34) : new Thickness(24, 6, 22, 30);
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
