using Lexplosion.Logic.Management.Addons;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Services.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Mvvm.Models.AddonsRepositories
{
    public enum AddonVersionsState
    {
        Hint,
        Loading,
        Ready,
        Empty,
        Error
    }

    public sealed class AddonDetailsModel : ObservableObject
    {
        private readonly Action<object> _installVersion;
        private readonly Gallery _gallery;
        private readonly Task _pageDataTask;
        private IReadOnlyList<object> _versions = Array.Empty<object>();
        private object _selectedVersion;
        private string _versionsError;
        private AddonVersionsState _versionsState;
        private bool _isInformationOpen;
        private int _selectedTabIndex;

        public InstanceAddon Addon { get; }

        public AddonPageData PageData { get; private set; }
        public bool HasGallery => PageData.Images.Count > 0;
        public bool IsPageLoaded { get; private set; }
        public bool IsPageLoadFailed { get; private set; }

        public bool IsInformationOpen
        {
            get => _isInformationOpen;
            private set
            {
                _isInformationOpen = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value)
                {
                    return;
                }

                _selectedTabIndex = value;
                OnPropertyChanged();
                if (value == 2)
                {
                    _ = LoadVersionsAsync();
                }
            }
        }

        public IReadOnlyList<object> Versions
        {
            get => _versions;
            private set
            {
                _versions = value;
                OnPropertyChanged();
            }
        }

        public object SelectedVersion
        {
            get => _selectedVersion;
            set
            {
                _selectedVersion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanInstallSelectedVersion));
            }
        }

        public bool CanInstallSelectedVersion => SelectedVersion != null;

        public AddonVersionsState VersionsState
        {
            get => _versionsState;
            private set
            {
                _versionsState = value;
                OnPropertyChanged();
            }
        }

        public string VersionsError
        {
            get => _versionsError;
            private set
            {
                _versionsError = value;
                OnPropertyChanged();
            }
        }

        public AddonDetailsModel(InstanceAddon addon, Action<object> installVersion, Gallery gallery)
        {
            Addon = addon ?? throw new ArgumentNullException(nameof(addon));
            _installVersion = installVersion;
            _gallery = gallery;
            PageData = AddonPageData.FromAddon(addon);
            _pageDataTask = LoadPageCoreAsync();
        }

        public Task LoadPageAsync()
        {
            return _pageDataTask;
        }

        private async Task LoadPageCoreAsync()
        {
#if DEBUG
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
#endif
            try
            {
                PageData = await AddonDetailsDataLoader.FetchPageAsync(Addon);
                OnPropertyChanged(nameof(PageData));
                OnPropertyChanged(nameof(HasGallery));
            }
            catch (Exception ex)
            {
                Runtime.DebugWrite("Addon page load failed: " + ex.GetType().Name);
                IsPageLoadFailed = true;
                OnPropertyChanged(nameof(IsPageLoadFailed));
            }
            finally
            {
                IsPageLoaded = true;
                OnPropertyChanged(nameof(IsPageLoaded));
#if DEBUG
                Lexplosion.QA.PerformanceTelemetry.Record("addon.data.fetch", (System.Diagnostics.Stopwatch.GetTimestamp() - started) * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
#endif
            }
        }

        public async Task LoadVersionsAsync()
        {
            if (VersionsState != AddonVersionsState.Hint && VersionsState != AddonVersionsState.Error)
            {
                return;
            }
            VersionsState = AddonVersionsState.Loading;

            try
            {
                var versions = await Task.Run(() => Addon.GetAllVersion()?.Values.ToArray() ?? Array.Empty<object>());
                Versions = Array.AsReadOnly(versions);
                SelectedVersion = versions.FirstOrDefault();
                VersionsState = versions.Length == 0 ? AddonVersionsState.Empty : AddonVersionsState.Ready;
            }
            catch (Exception ex)
            {
                VersionsError = ex.Message;
                VersionsState = AddonVersionsState.Error;
            }
        }

        public void OpenImage(object image)
        {
            _gallery.ChangeContext(PageData.Images);
            _gallery.SelectImage(image);
        }

        public void InstallSelectedVersion()
        {
            if (SelectedVersion != null)
            {
                _installVersion?.Invoke(SelectedVersion);
            }
        }

        public void ToggleInformation()
        {
            IsInformationOpen = !IsInformationOpen;
        }

        public void CloseInformation()
        {
            IsInformationOpen = false;
        }
    }
}
