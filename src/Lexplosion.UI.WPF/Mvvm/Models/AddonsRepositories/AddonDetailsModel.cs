using Lexplosion.Logic.Management.Addons;
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
        private readonly AddonPageData _initialPageData;
        private readonly Task<AddonPageData> _pageDataTask;
        private IReadOnlyList<object> _versions = Array.Empty<object>();
        private object _selectedVersion;
        private string _versionsError;
        private AddonVersionsState _versionsState;
        private bool _isPageLoaded;
        private bool _isInformationOpen;
        private int _selectedTabIndex;

        public InstanceAddon Addon { get; }

        public AddonPageData PageData => IsPageLoaded && _pageDataTask.Status == TaskStatus.RanToCompletion ? _pageDataTask.Result : _initialPageData;
        public bool HasGallery => PageData.Images.Count > 0;

        public bool IsPageLoaded
        {
            get => _isPageLoaded;
            private set
            {
                _isPageLoaded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageData));
                OnPropertyChanged(nameof(HasGallery));
            }
        }

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

        public AddonDetailsModel(InstanceAddon addon, Action<object> installVersion)
        {
            Addon = addon ?? throw new ArgumentNullException(nameof(addon));
            _installVersion = installVersion;
            _initialPageData = AddonPageData.FromAddon(addon);
            _pageDataTask = Task.Run(() => AddonDetailsDataLoader.FetchPage(addon));
        }

        public async Task LoadPageAsync()
        {
            try
            {
                await _pageDataTask;
            }
            finally
            {
                if (!IsPageLoaded)
                {
                    IsPageLoaded = true;
                }
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
