using Lexplosion.Global;
using Lexplosion.Gui.Models;
using Lexplosion.Gui.Models.InstanceForm;
using Lexplosion.Gui.Stores;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Objects;
using System.Collections.Generic;
using System.Windows;

namespace Lexplosion.Gui.ViewModels
{
    public class MainViewModel : VMBase
    {
        public static readonly NavigationStore NavigationStore = new NavigationStore();
        public static bool IsInstanceRunning = false;

        private bool _isAuthorized;
        public bool IsAuthorized 
        {
            get => _isAuthorized; set 
            {
                _isAuthorized = value;
                OnPropertyChanged(nameof(IsAuthorized));
            }
        }

        public bool IsAuth { get => UserData.IsAuthorized; }

        public MainModel Model { get; } 
        public VMBase CurrentViewModel => NavigationStore.CurrentViewModel;

        private string _nickname;

        private RelayCommand _closeCommand;
        private RelayCommand _hideCommand;

        #region props

        public string Nickname 
        {
            get => _nickname; set 
            {
                _nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public object InstanceForms { get; private set; }
        #endregion

        #region commands
        public RelayCommand CloseCommand 
        {
            get => _closeCommand ?? (_closeCommand = new RelayCommand(obj => 
            {
                Run.Exit();
            }));
        }

        public RelayCommand HideCommand 
        {
            get => _hideCommand ?? (_hideCommand = new RelayCommand(obj => 
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }));
        }
        #endregion

        public MainViewModel()
        {
            Model = new MainModel();
            NavigationStore.CurrentViewModel = new AuthViewModel(this);
            InstancesLoading();
            NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        private void InstancesLoading()
        {
            if (UserData.Instances.Record.Keys.Count == 0)
                return;

            string description, imageUrl, author, outsideInstanceId;
            InstanceSource source;

            foreach (var key in UserData.Instances.Record.Keys)
            {
                description = "This modpack is not have description...";
                imageUrl = "pack://application:,,,/assets/images/icons/non_image.png";
                author = "by NightWorld";
                outsideInstanceId = string.Empty;
                source = InstanceSource.Local;

                var categories = new List<Category>();

                if (UserData.Instances.Assets.ContainsKey(key))
                {
                    if (UserData.Instances.Assets[key] != null)
                    {
                        description = UserData.Instances.Assets[key].description;
                        imageUrl = WithDirectory.DirectoryPath + "/instances-assets/" + UserData.Instances.Assets[key].mainImage;
                        author = UserData.Instances.Assets[key].author;
                        source = UserData.Instances.Record[key].Type;

                        if (UserData.Instances.Assets[key].categories != null)
                        {
                            categories = UserData.Instances.Assets[key].categories;
                        }

                        foreach (var key1 in UserData.Instances.ExternalIds.Keys)
                        {
                            if (UserData.Instances.ExternalIds[key1] == key)
                            {
                                outsideInstanceId = key1;
                            }
                        }
                    }
                }
                MainModel.AddedInstanceForms.Add(
                    new InstanceFormViewModel(
                        new InstanceFormModel(
                            new InstanceProperties
                            {
                                Name = UserData.Instances.Record[key].Name,
                                Type = source,
                                LocalId = key,
                                InstanceAssets = new InstanceAssets()
                                {
                                    author = author,
                                    description = description,
                                    categories = categories
                                },
                                Id = outsideInstanceId,
                                Logo = Utilities.GetImage(imageUrl),
                                IsDownloadingInstance = false,
                                IsInstalled = UserData.Instances.Record[key].IsInstalled,
                                UpdateAvailable = UserData.Instances.Record[key].UpdateAvailable,
                                IsInstanceAddedToLibrary = true
                            }
                        )
                    )
                );
            }
        }
    }
}
