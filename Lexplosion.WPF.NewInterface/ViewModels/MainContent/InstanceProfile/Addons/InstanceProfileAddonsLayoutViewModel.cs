using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Commands;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using Lexplosion.WPF.NewInterface.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.ViewModels.MainContent.InstanceProfile
{
    public class ObservableHashSet<T> : HashSet<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";
        private SimpleMonitor _monitor = new SimpleMonitor();


        #region Events


        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        #region INotifyPropertyChanged implementation


        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }
        }


        #endregion INotifyPropertyChanged implementation


        #endregion Events


        #region Public Methods


        public new void Add(T item) 
        {
            CheckReentrancy();
            base.Add(item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged( NotifyCollectionChangedAction.Add, item, this.Count - 1);
        }

        public new void Clear() 
        {
            CheckReentrancy();
            base.Clear();

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }


        #endregion Public Methods


        #region Protected Methods


        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        /// <summary>
        /// Disallow reentrant attempts to change this collection. E.g. a event handler
        /// of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        /// <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        /// </code>
        /// </remarks>
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }


        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException();
            }
        }


        #endregion Protected Methods


        #region Private Methods


        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }


        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        
        
        #endregion Private Methods


        #region Private Types


        private sealed class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy { get { return _busyCount > 0; } }

            int _busyCount;
        }


        #endregion Private Types
    }

    public sealed class InstanceAddonsContainerModel 
    {
        public AddonType Type { get; } 

        private ObservableCollection<InstanceAddon> _addonsList = new ObservableCollection<InstanceAddon>();
        public IEnumerable<InstanceAddon> AddonsList { get => _addonsList; }


        private bool _isAddonsLoaded = true;
        public bool IsAddonsLoaded 
        {
            get => _isAddonsLoaded; private set 
            {
                _isAddonsLoaded = value;
            }
        }

        private readonly BaseInstanceData _baseInstanceData;
        private readonly InstanceModelBase _instanceModelBase;

        public InstanceAddonsContainerModel(AddonType type, InstanceModelBase instanceModelBase)
        {
            Type = type;
            _instanceModelBase = instanceModelBase;
            _baseInstanceData = instanceModelBase.InstanceData;
        }


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
        public void SetAddons(IEnumerable<InstanceAddon> addons) 
        {
            App.Current.Dispatcher?.Invoke(() => { 
                foreach (var addon in addons)
                {
                    _addonsList.Add(addon);
                }
                //IsAddonsLoaded = !true;
            });
        }

        public void Reload() 
        {
            var installedAddons = InstanceAddon.GetInstalledAddons(Type, _baseInstanceData);
            _addonsList.Clear();
            //IsAddonsLoaded = !false;
            SetAddons(installedAddons);
        }

        public void UpdateAddon(object instanceAddon) 
        {
            if (instanceAddon is InstanceAddon)
                ((InstanceAddon)instanceAddon).Update();
        }

        public void UninstallAddon(object instanceAddon)
        {
            if (instanceAddon is InstanceAddon)
                ((InstanceAddon)instanceAddon).Delete();
        }


        #endregion Public Methods
    }

    public sealed class InstanceAddonsContainerViewModel : ViewModelBase
    {
        public InstanceAddonsContainerModel Model { get; private set; }

        private readonly InstanceModelBase _instanceModelBase;

        #region Commands


        private RelayCommand _openMarketCommand;
        public ICommand OpenMarketCommand
        {
            get => RelayCommand.GetCommand(ref _openMarketCommand, (obj) => { });
        }

        private RelayCommand _reloadCommand;
        public ICommand ReloadCommand
        {
            get => RelayCommand.GetCommand(ref _reloadCommand, (obj) => { });
        }

        private RelayCommand _openFolderCommand;
        public ICommand OpenFolderCommand
        {
            get => RelayCommand.GetCommand(ref _openFolderCommand, (obj) => { _instanceModelBase.OpenFolder(); });
        }

        private RelayCommand _updateCommand;
        public ICommand UpdateCommand 
        {
            get => RelayCommand.GetCommand(ref _updateCommand, Model.UpdateAddon);
        }

        private RelayCommand _uninstallCommand;
        public ICommand UninstallCommand
        {
            get => RelayCommand.GetCommand(ref _uninstallCommand, (obj) => 
            {
                var dialogViewModel = new DialogBoxViewModel("delete", "delete",
                (obj) =>
                {
                    Model.UninstallAddon(obj);
                }, (obj) => { ModalNavigationStore.Instance.Close(); });
                ModalNavigationStore.Instance.Open(dialogViewModel);
            });
        }


        #endregion Commands


        public InstanceAddonsContainerViewModel(AddonType addonType, InstanceModelBase instanceModelBase)
        {
            _instanceModelBase = instanceModelBase;
            Model = new InstanceAddonsContainerModel(addonType, instanceModelBase);
        }
    }

    public sealed class InstanceProfileAddonsLayoutViewModel : ContentLayoutViewModelBase
    {
        private readonly ViewModelBase _modsViewModel;
        private readonly ViewModelBase _resourcepacksViewModel;
        private readonly ViewModelBase _mapsViewModel;
        private readonly ViewModelBase _shadersViewModel;

        public InstanceProfileAddonsLayoutViewModel(InstanceModelBase instanceModelBase) : base()
        {
            HeaderKey = "Addons";
            _modsViewModel = new InstanceAddonsContainerViewModel(AddonType.Mods, instanceModelBase);
            _resourcepacksViewModel = new InstanceAddonsContainerViewModel(AddonType.Resourcepacks, instanceModelBase);
            _mapsViewModel = new InstanceAddonsContainerViewModel(AddonType.Maps, instanceModelBase);
            _shadersViewModel = new InstanceAddonsContainerViewModel(AddonType.Shaders, instanceModelBase);
            InitAddonsTabMenu(instanceModelBase);
        }

        private void InitAddonsTabMenu(InstanceModelBase instanceModelBase)
        {
            _tabs.Add(new TabItemModel { TextKey = "Mods", Content = _modsViewModel, IsSelected = true });
            _tabs.Add(new TabItemModel { TextKey = "Resourcepacks", Content = _resourcepacksViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Maps", Content = _mapsViewModel });
            _tabs.Add(new TabItemModel { TextKey = "Shaders", Content = _shadersViewModel });
        }
    }
}
