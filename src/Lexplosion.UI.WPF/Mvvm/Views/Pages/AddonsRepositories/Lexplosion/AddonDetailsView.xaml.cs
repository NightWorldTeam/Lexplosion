using Lexplosion.Logic.Management.Addons;
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
            view.MinecraftVersionsItems.ItemsSource = Labels(
                string.IsNullOrWhiteSpace(view.Addon?.GameVersion)
                    ? Array.Empty<string>() : new[] { view.Addon.GameVersion }, 8);
            view.ModloadersItems.ItemsSource = Labels(view.Addon?.DisplayModloaders, 6);
            view.GalleryEmptyText.Text = view.L("AddonDetailsGalleryLoading");
            view.GalleryEmptyText.Visibility = Visibility.Visible;
            view.CategoryItems.ItemsSource = Labels(
                view.Addon?.Categories?.Select(category => category.Name), 10);
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

        // Use the launcher's active language dictionary, including runtime language changes.
        private string L(string key) => TryFindResource(key) as string ?? key;

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


        private void OnInformationClick(object sender, RoutedEventArgs e)
        {
            IsInformationOpen = !IsInformationOpen;
        }

        private void OnInformationScrimClick(object sender, MouseButtonEventArgs e)
        {
            IsInformationOpen = false;
            e.Handled = true;
        }

    }
}
