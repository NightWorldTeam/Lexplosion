using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent.InstanceProfile
{
    public sealed class InstanceAddonsContainerModel : ViewModelBase
    {
        private readonly BaseInstanceData _baseInstanceData;
        private readonly InstanceModelBase _instanceModelBase;
        private ObservableCollection<InstanceAddon> _addonsList = new();
        
        
        #region Properties


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
        private bool _isSearchEnabled = false;
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


        #endregion Properties


        #region Constructors


        public InstanceAddonsContainerModel(AddonType type, InstanceModelBase instanceModelBase)
        {
            Type = type;
            _instanceModelBase = instanceModelBase;
            _baseInstanceData = instanceModelBase.InstanceData;

            Runtime.TaskRun(() => {

                var instanceAddons = InstanceAddon.GetInstalledAddons(type, _baseInstanceData);

                App.Current.Dispatcher.Invoke(() => {
                    _addonsList = new ObservableCollection<InstanceAddon>(instanceAddons);
                    InstanceAddonCollectionViewSource.Source = _addonsList;
                    OnPropertyChanged(nameof(AddonsList));
                    IsAddonsLoading = false;
                    OnPropertyChanged(nameof(AddonsCount));

                    Runtime.DebugWrite(AddonsCount);
                });
            });
        }


        #endregion Constructors


        #region Public Methods


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
                var installedAddons = InstanceAddon.GetInstalledAddons(Type, _baseInstanceData);
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
                (instanceAddon as InstanceAddon).Update();
        }

        /// <summary>
        /// Удаляет addon.
        /// </summary>
        /// <param name="instanceAddon">Addon который нужно удалить</param>
        public void UninstallAddon(object instanceAddon)
        {
            if (instanceAddon is InstanceAddon)
                (instanceAddon as InstanceAddon).Delete();
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
            InstanceAddonCollectionViewSource.View.Filter = (m => (m as InstanceAddon).Name.IndexOf(value, System.StringComparison.InvariantCultureIgnoreCase) > -1);
        }


        #endregion Private Methods
    }
}
