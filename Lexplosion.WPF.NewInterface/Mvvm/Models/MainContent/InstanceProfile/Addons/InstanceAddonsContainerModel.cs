using Lexplosion.Logic.Management.Addons;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile
{
    public sealed class InstanceAddonsContainerModel : ViewModelBase
    {
        private static string[] _sortByList =
        {
            "Name", "Author", "FileName"
        };


        private BaseInstanceData _baseInstanceData;
        private readonly InstanceModelBase _instanceModelBase;
        private ObservableCollection<InstanceAddon> _addonsList = new();

        private readonly Func<string> GetDirectoryPath;
        private readonly string _directoryPath;


        #region Properties


        public Action<IEnumerable<string>> ImportAction { get; }

        public bool IsBigImportLoading { get; private set; }

        public IEnumerable<string> AvailableImportFileExtensions { get; }

        public string[] SortByList => _sortByList;
        /// <summary>
        /// Тип аддонов.
        /// </summary>
        public AddonType Type { get; }
        /// <summary>
        /// Список установленных аддонов.
        /// </summary>
        public IReadOnlyCollection<InstanceAddon> AddonsList { get => _addonsList; }
        /// <summary>
        /// Прокси-сервер для CollectionView класса
        /// </summary>
        public CollectionViewSource InstanceAddonCollectionViewSource { get; } = new();
        /// <summary>
        /// Количетсво установленных адднов.
        /// </summary>
        public int AddonsCount => AddonsList.Count;
        /// <summary>
        /// Является ли лист пустым.
        /// Пустым - AddonsCount = 0, IsAddonsLoading = false. 
        /// </summary>
        public bool IsEmptyAddonsList { get => AddonsCount == 0 && !IsAddonsLoading; }

        /// <summary>
        /// Идет ли процесс загрузки аддонов.
        /// </summary>
        private bool _isAddonsLoaded = true;
        public bool IsAddonsLoading
        {
            get => _isAddonsLoaded; private set
            {
                _isAddonsLoaded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Включен ли поиск.
        /// </summary>
        private bool _isSearchEnabled = true;
        public bool IsSearchEnabled
        {
            get => _isSearchEnabled; set
            {
                _isSearchEnabled = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Текст в поле поиска.
        /// </summary>
        private string _searchBoxText;
        public string SearchBoxText
        {
            get => _searchBoxText; set
            {
                _searchBoxText = value;
                OnPropertyChanged();
                OnSearchBoxTextChanged(value);
            }
        }

        private string _selectedSortByParam;
        public string SelectedSortByParam
        {
            get => _selectedSortByParam; set
            {
                _selectedSortByParam = value;
                OnPropertyChanged();
                OnSearchBoxTextChanged(_searchBoxText);
            }
        }


        #endregion Properties


        #region Constructors


        public InstanceAddonsContainerModel(AddonType type, InstanceModelBase instanceModelBase, Func<string> getDirectoryPath = null)
        {
            Type = type;
            _selectedSortByParam = SortByList[0];
            _instanceModelBase = instanceModelBase;

            string folderName = string.Empty;

            switch (type)
            {
                case AddonType.Mods:
                    {
                        folderName = "mods";
                        AvailableImportFileExtensions = [".jar"];
                    }
                    break;
                case AddonType.Resourcepacks:
                    {
                        folderName = "resourcepacks";
                        AvailableImportFileExtensions = [".zip"];
                    }
                    break;
                case AddonType.Maps:
                    {
                        folderName = "saves";
                        AvailableImportFileExtensions = [".zip"];
                    }
                    break;
                case AddonType.Shaders:
                    {
                        folderName = "shaders";
                        AvailableImportFileExtensions = [".zip"];
                    }
                    break;
                default:
                    break;
            };

            _directoryPath = $"{getDirectoryPath()}\\{folderName}";

            Runtime.TaskRun(() =>
            {
                _baseInstanceData = instanceModelBase.BaseData;
                var instanceAddons = AddonsManager.GetManager(_baseInstanceData).GetInstalledAddons(type);

                App.Current.Dispatcher.Invoke(() => 
                {
                    _addonsList = new ObservableCollection<InstanceAddon>(instanceAddons);
                    InstanceAddonCollectionViewSource.Source = _addonsList;
                    OnPropertyChanged(nameof(AddonsList));
                    IsAddonsLoading = false;
                    OnPropertyChanged(nameof(AddonsCount));

                    AddonsManager.GetManager(_baseInstanceData).StartWathingDirecoty();
                    AddonsManager.GetManager(_baseInstanceData).AddonAdded += InstanceAddon_AddonAdded;
                    AddonsManager.GetManager(_baseInstanceData).AddonRemoved += InstanceAddon_AddonRemoved;
                });
            });

            ImportAction = (files) => 
            {
                Runtime.TaskRun(() => 
                {
                    if (files.Count() > 10)
                        IsAddonsLoading = true;

                    AddonsManager.GetManager(_baseInstanceData).AddAddons(files, type, out var addons);

                    if (addons == null)
                        return;

                    App.Current.Dispatcher?.Invoke(() => 
                    { 
                        _addonsList = new ObservableCollection<InstanceAddon>(addons);
                        OnPropertyChanged(nameof(AddonsList));
                        IsAddonsLoading = false;
                    });
                });
            };
        }


        #endregion Constructors


        #region Public Methods


        public void OpenFolder()
        {

            try
            {
                Process.Start("explorer", _directoryPath);
            }
            catch
            {
                Process.Start("explorer", GetDirectoryPath());
            }
        }


        /// <summary>
        /// Добавляет новый аддон в конец списка.
        /// </summary>
        /// <param name="addon">Аддон которые мы желаем добавить.</param>
        public void SetAddon(InstanceAddon addon)
        {
            _addonsList.Add(addon);
        }

        /// <summary>
        /// Добавляет новые аддоны в конец списка.
        /// </summary>
        /// <param name="addons">Коллекция с аддонами</param>
        public void SetAddons(IEnumerable<InstanceAddon> addons, bool isClearCollection = false, bool isDisableLoading = false)
        {
            App.Current.Dispatcher?.Invoke(() =>
            {
                if (isClearCollection)
                    _addonsList.Clear();

                foreach (var addon in addons)
                {
                    _addonsList.Add(addon);
                }

                if (isDisableLoading)
                    IsAddonsLoading = false;
            });
        }

        /// <summary>
        /// Перезагружает данные для AddonsList.
        /// </summary>
        public void Reload()
        {
            IsAddonsLoading = true;
            Runtime.TaskRun(() =>
            {
                var installedAddons = AddonsManager.GetManager(_baseInstanceData).GetInstalledAddons(Type);
                //IsAddonsLoaded = !false;
                SetAddons(installedAddons, true, true);
            });
        }

        /// <summary>
        /// Обновляет addon.
        /// </summary>
        /// <param name="instanceAddon">Addon который нужно обновить</param>
        public void UpdateAddon(object instanceAddon)
        {
            if (instanceAddon is InstanceAddon)
            {
                Runtime.TaskRun(() => {
                    (instanceAddon as InstanceAddon).Update();
                });
            }
        }

        /// <summary>
        /// Удаляет addon.
        /// </summary>
        /// <param name="instanceAddon">Addon который нужно удалить</param>
        public void UninstallAddon(object instanceAddon)
        {
            if (instanceAddon is InstanceAddon addon)
            {
                Runtime.TaskRun(() => {
                    addon.Delete();
                });
                _addonsList.Remove(addon);
            }
        }


        public void OpenExternalResource(InstanceAddon addon)
        {
            try
            {
                Process.Start(addon.WebsiteUrl);
            }
            catch (Exception ex)
            {
            }
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Обновляется при изменении текста в searchbox. При выполнении фильтрует значение имени.
        /// </summary>
        /// <param name="value"></param
        private void OnSearchBoxTextChanged(string value)
        {
            if (InstanceAddonCollectionViewSource.View == null)
                return;

            value ??= string.Empty;
            if (SelectedSortByParam == "Name")
            {
                InstanceAddonCollectionViewSource.View.Filter = (m => (m as InstanceAddon).Name?.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            }
            else if (SelectedSortByParam == "Author")
            {
                InstanceAddonCollectionViewSource.View.Filter = (m => (m as InstanceAddon).Author?.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            }
            else if (SelectedSortByParam == "FileName") 
            {
                InstanceAddonCollectionViewSource.View.Filter = (m => (m as InstanceAddon).FileName?.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
            }
        }

        private void InstanceAddon_AddonRemoved(InstanceAddon obj)
        {
            App.Current.Dispatcher.Invoke(() => {
                _addonsList.Remove(obj);
            });
        }

        private void InstanceAddon_AddonAdded(InstanceAddon obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _addonsList.Add(obj);
            });
        }


        #endregion Private Methods
    }
}
