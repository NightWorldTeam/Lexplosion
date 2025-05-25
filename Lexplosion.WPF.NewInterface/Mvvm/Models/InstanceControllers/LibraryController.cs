using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public sealed class LibraryController : ObservableObject, ILibraryInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;


        private readonly AppCore _appCore;
        private readonly Action<InstanceClient> _exportFunc;
        private readonly Action<InstanceModelBase> _setRunningGame;
        private readonly ClientsManager _clientsManager;

        private ObservableCollection<InstanceModelBase> _instances = new();
        private ObservableCollection<InstancesGroup> _groups = new();


        #region Properties


        /// <summary>
        /// Сборки выбранной группы
        /// </summary>
        public IReadOnlyCollection<InstanceModelBase> Instances { get => _instances; }
        /// <summary>
        /// Выбранная группа
        /// </summary>
        public InstancesGroup SelectedGroup { get; private set; }
        /// <summary>
        /// Группы сборок
        /// </summary>
        public IReadOnlyCollection<InstancesGroup> InstancesGroups { get => _groups; }


        #endregion Properties


        #region Constructors


        public LibraryController(AppCore appCore, ClientsManager clientsManager, Action<InstanceClient> export, Action<InstanceModelBase> setRunningGame)
        {
            _appCore = appCore;
            _clientsManager = clientsManager;

            _groups = new(_clientsManager.GetExistsGroups());

            InstanceModelBase.GlobalAddedToLibrary += (im) => Add(im);
            InstanceModelBase.GlobalDeletedEvent += Remove;
            InstanceModelBase.GlobalGroupRemovedEvent += Remove;

            _exportFunc = export;
            _setRunningGame = setRunningGame;
        }


        #endregion Constructors


        #region Public Methods


        public void Add(InstanceModelBase instanceModelBase, [CallerMemberName] string member = "")
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _instances.Add(instanceModelBase);
                InstanceAdded?.Invoke(instanceModelBase);
                OnPropertyChanged(nameof(Instances));
            });
        }

        public InstanceModelBase? Add(InstanceClient instanceClient, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame, group: SelectedGroup);
                instanceModelBase = new InstanceModelBase(args);
                Add(instanceModelBase);
            });

            return instanceModelBase;
        }

        public InstanceModelBase? Add(InstanceClient instanceClient, InstanceDistribution instanceDistribution, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame, instanceDistribution, group: SelectedGroup);
                instanceModelBase = new InstanceModelBase(args);
                Add(instanceModelBase);
            });

            return instanceModelBase;
        }

        public InstanceModelBase? Add(InstanceClient instanceClient, ImportData? importData, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame, importData: importData, group: SelectedGroup);
                instanceModelBase = new InstanceModelBase(args);
                Add(instanceModelBase);
            });

            return instanceModelBase;
        }

        public void Remove(InstanceClient instanceClient)
        {
            var i = Instances.Where(i => i.CheckInstanceClient(instanceClient)).ToArray()[0];
            Remove(i);
        }

        public void Remove(InstanceModelBase instanceModelBase)
        {
            Runtime.TaskRun(() =>
            {
                // задержка, чтобы успела анимация проиграть, так или иначе сборка удалена, далее только "визуальная часть".
                Thread.Sleep(320);
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (_instances.Contains(instanceModelBase))
                    {
                        _instances.Remove(instanceModelBase);
                    }
                });
            });
        }

        public void Clear()
        {
            _instances.Clear();
        }


        public InstanceModelBase? GetByInstanceClient(InstanceClient? instanceClient)
        {
            if (instanceClient == null)
                return null;

            return _instances.FirstOrDefault(i => i.Equals(instanceClient));
        }

        public void GroupItemsChanged()
        {
            _instances.Clear();

            foreach (var ic in SelectedGroup.Clients)
            {
                Add(ic);
            }
        }

        public void SelectGroup(InstancesGroup instancesGroup)
        {
            if (SelectedGroup != null)
            {
                SelectedGroup.NewInstanceAdded -= GroupItemsChanged;
            }

            SelectedGroup = instancesGroup;
            SelectedGroup.IsSelected = true;
            SelectedGroup.NewInstanceAdded += GroupItemsChanged;

            GroupItemsChanged();

            OnPropertyChanged(nameof(SelectedGroup));
            OnPropertyChanged(nameof(Instances));
        }

        public void AddGroup(InstancesGroup instancesGroup)
        {
            _groups.Add(instancesGroup);
        }

        public void RemoveGroup(InstancesGroup instancesGroup)
        {
            _groups.Remove(instancesGroup);
            _clientsManager.DeleteGroup(instancesGroup);
        }


        #endregion Public Methods
    }
}
