using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Controls;
using Lexplosion.UI.WPF.Mvvm.ViewModels.AddonsRepositories;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.Views.Pages.AddonDetails
{
    /// <summary>
    /// Project page for catalog addons. The existing repository owns installation,
    /// filters and download state; this view only loads rich project information.
    /// </summary>
    public partial class AddonDetailsView : UserControl
    {
        public static readonly DependencyProperty AddonProperty = DependencyProperty.Register(
            nameof(Addon), typeof(InstanceAddon), typeof(AddonDetailsView),
            new PropertyMetadata(null, OnAddonPropertyChanged));

        public InstanceAddon Addon
        {
            get => (InstanceAddon)GetValue(AddonProperty);
            set => SetValue(AddonProperty, value);
        }

        public static readonly DependencyProperty IsInformationOpenProperty = DependencyProperty.Register(
            nameof(IsInformationOpen), typeof(bool), typeof(AddonDetailsView), new PropertyMetadata(false));

        public bool IsInformationOpen
        {
            get => (bool)GetValue(IsInformationOpenProperty);
            set => SetValue(IsInformationOpenProperty, value);
        }

        private string[] _minecraftVersions = Array.Empty<string>();
        private string[] _modloaders = Array.Empty<string>();
        private string[] _categories = Array.Empty<string>();
        private bool _showAllVersions;
        private bool _showAllLoaders;
        private bool _showAllCategories;
        private int _generation;
        private int _versionsLoadedForGeneration = -1;

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
            view.IsInformationOpen = false;
            view._versionsLoadedForGeneration = -1;
            view.VersionsList.ItemsSource = null;
            view.VersionsStatus.Text = view.L("AddonDetailsVersionsHint");
            view.GalleryItems.ItemsSource = null;
            // Show only grounded metadata. The full Modrinth response can fill these later.
            view._showAllVersions = view._showAllLoaders = view._showAllCategories = false;
            view._minecraftVersions = Labels(string.IsNullOrWhiteSpace(view.Addon?.GameVersion)
                ? Array.Empty<string>() : new[] { view.Addon.GameVersion });
            view._modloaders = Labels(view.Addon?.DisplayModloaders);
            view.UpdateMetadataLabels();
            view.GalleryEmptyText.Text = view.L("AddonDetailsGalleryLoading");
            view.GalleryEmptyText.Visibility = Visibility.Visible;
            view._categories = Labels(view.Addon?.Categories?.Select(category => category.Name));
            view.UpdateMetadataLabels();
            view.DetailsTabs.SelectedIndex = 0;
            if (view.IsLoaded && view.IsVisible && view.Addon != null)
                _ = view.RenderAddonAsync(view.Addon, view._generation);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
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
            CleanupBrowser();
        }

        private static string[] Labels(IEnumerable<string> values)
        {
            var labels = (values ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return labels.Length == 0 ? new[] { "—" } : labels;
        }

        private void UpdateMetadataLabels()
        {
            SetMetadataLabels(MinecraftVersionsItems, MoreMinecraftVersionsButton,
                _minecraftVersions, 7, _showAllVersions);
            SetMetadataLabels(ModloadersItems, MoreModloadersButton,
                _modloaders, 6, _showAllLoaders);
            SetMetadataLabels(CategoryItems, MoreCategoriesButton,
                _categories, 10, _showAllCategories);
        }

        private void SetMetadataLabels(ItemsControl list, AdvancedButton more,
            string[] values, int limit, bool expanded)
        {
            list.ItemsSource = expanded ? values : values.Take(limit).ToArray();
            more.Visibility = values.Length > limit ? Visibility.Visible : Visibility.Collapsed;
            more.Text = expanded ? L("ShowLess") :
                L("More") + " (+" + (values.Length - limit) + ")";
        }

        private void OnMoreMetadataClick(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, MoreMinecraftVersionsButton))
                _showAllVersions = !_showAllVersions;
            else if (ReferenceEquals(sender, MoreModloadersButton))
                _showAllLoaders = !_showAllLoaders;
            else if (ReferenceEquals(sender, MoreCategoriesButton))
                _showAllCategories = !_showAllCategories;
            UpdateMetadataLabels();
        }

        // Use the launcher's active language dictionary, including runtime language changes.
        private string L(string key) => TryFindResource(key) as string ?? key;

        private async void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, DetailsTabs)) return;
            // A native HWND must not cover the WPF gallery or version list.
            UpdateBrowserHitTesting();
            if (DetailsTabs.SelectedIndex != 2) return;
            var addon = Addon;
            int generation = _generation;
            if (addon == null || _versionsLoadedForGeneration == generation) return;
            _versionsLoadedForGeneration = generation;
            VersionsStatus.Text = L("AddonDetailsVersionsLoading");
            try
            {
                var versions = await Task.Run(() => addon.GetAllVersion()?.Values.ToArray() ?? Array.Empty<object>());
                if (generation != _generation || !ReferenceEquals(addon, Addon)) return;
                VersionsList.ItemsSource = versions;
                if (versions.Length > 0) VersionsList.SelectedIndex = 0;
                VersionsStatus.Text = L(versions.Length == 0 ? "AddonDetailsVersionsEmpty" : "AddonDetailsVersionsSelect");
            }
            catch (Exception ex)
            {
                if (generation == _generation)
                {
                    _versionsLoadedForGeneration = -1;
                    VersionsStatus.Text = string.Format(L("AddonDetailsVersionsError"), ex.Message);
                }
            }
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


        private void UpdateBrowserHitTesting()
        {
            // WebView2 is an HWND and otherwise covers the compact WPF drawer.
            if (_browser != null)
                _browser.Visibility = !IsInformationOpen && DetailsTabs.SelectedIndex == 0 &&
                    LoadingOverlay.Visibility == Visibility.Collapsed
                    ? Visibility.Visible : Visibility.Hidden;
        }

        private void OnInformationClick(object sender, RoutedEventArgs e)
        {
            IsInformationOpen = !IsInformationOpen;
            UpdateBrowserHitTesting();
        }

        private void OnInformationScrimClick(object sender, MouseButtonEventArgs e)
        {
            IsInformationOpen = false;
            UpdateBrowserHitTesting();
            e.Handled = true;
        }

    }
}
