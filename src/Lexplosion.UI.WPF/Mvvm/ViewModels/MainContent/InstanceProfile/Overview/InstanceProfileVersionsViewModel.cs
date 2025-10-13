using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
{
    public class InstanceProfileVersionsModel : ObservableObject
    {
        public ObservableCollection<InstanceVersion> Versions { get; private set; }

        public InstanceModelBase InstanceModel { get; }

        public bool IsLoading { get; private set; } = true;

        public InstanceProfileVersionsModel(InstanceModelBase instanceModelBase)
        {
            InstanceModel = instanceModelBase;
            InstanceModel.DownloadStarted += OnDownloadStarted;
            InstanceModel.DownloadComplited += OnInstanceVersionDownloadComplited;

            Runtime.TaskRun(() =>
            {
                var versions = instanceModelBase.GetInstanceVersions();
                App.Current.Dispatcher.Invoke(() =>
                {
                    Versions = new(versions);
                    OnPropertyChanged(nameof(Versions));
                    if (instanceModelBase.IsInstalled || instanceModelBase.InLibrary)
                        UpdateVersionStates();

                    if (InstanceModel.IsDownloading)
                    {
                        var version = Versions.FirstOrDefault(i => i.Id == InstanceModel.ClientVersion);

                        if (version != null)
                        {
                            version.IsDownloading = true;
                        }
                    }

                    IsLoading = false;
                    OnPropertyChanged(nameof(IsLoading));
                });
            });
        }

        private void OnDownloadStarted()
        {
            if (Versions == null || Versions.Count() == 0)
                return;

            if (Versions.FirstOrDefault(i => i.IsDownloading) == null)
            {
                var version = Versions.FirstOrDefault(i => i.Id == InstanceModel.ClientVersion);

                if (version != null)
                {
                    version.IsDownloading = true;
                }
            }
        }

        public void Install(InstanceVersion instanceVersion)
        {
            instanceVersion.IsDownloading = true;
            InstanceModel.Download(instanceVersion.Id);
        }

        private void OnInstanceVersionDownloadComplited(InstanceInit arg1, System.Collections.Generic.IEnumerable<string> arg2, bool arg3)
        {
            var version = Versions.FirstOrDefault(i => i.IsDownloading);

            if (version != null)
            {
                version.IsDownloading = false;
            }

            if (arg1 == InstanceInit.IsCancelled)
            {
                version.CanInstall = true;
            }
        }

        public void VisitWebsite(InstanceVersion instanceVersion)
        {
            if (InstanceModel.Source == InstanceSource.Modrinth)
            {
                System.Diagnostics.Process.Start($"{InstanceModel.WebsiteUrl}/version/{instanceVersion.VersionNumber}");
            }
            else if (InstanceModel.Source == InstanceSource.Curseforge)
            {
                System.Diagnostics.Process.Start($"{InstanceModel.WebsiteUrl}/files/{instanceVersion.Id}");
            }
        }

        #region Private Methods


        private void UpdateVersionStates()
        {
            foreach (var version in Versions)
            {
                version.CanInstall = true;
                if (version.Id == InstanceModel.ClientVersion && InstanceModel.IsInstalled)
                {
                    version.CanInstall = false;
                }
            }
        }


        #endregion Private Methoods
    }

    public class InstanceProfileVersionsViewModel : ViewModelBase
    {
        public InstanceProfileVersionsModel Model { get; }


        #region Commands


        private RelayCommand _installInstanceVersionCommand;
        public ICommand InstallInstanceVersionCommand
        {
            get => RelayCommand.GetCommand<InstanceVersion>(ref _installInstanceVersionCommand, Model.Install);
        }

        private RelayCommand _visitWebsiteCommand;
        public ICommand VisitWebsiteCommand
        {
            get => RelayCommand.GetCommand<InstanceVersion>(ref _visitWebsiteCommand, Model.VisitWebsite);
        }


        #endregion Commands


        public InstanceProfileVersionsViewModel(InstanceModelBase instanceModelBase)
        {
            Model = new(instanceModelBase);
        }
    }
}
