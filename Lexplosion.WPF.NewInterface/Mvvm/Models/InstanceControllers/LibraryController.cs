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
        private readonly Func<InstanceClient, InstanceModelBase> _getInstanceModelByInstanceClient;
        private readonly Action<InstanceModelBase> _addInstanceModel;
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
        private InstancesGroup _selectedGroup;
        public InstancesGroup SelectedGroup
        {
            get => _selectedGroup; set
            {
                //if (_selectedGroup != null) 
                //{
                //    _selectedGroup.NewInstanceAdded -= OnNewInstanceAddedToGroup;
                //}

                _selectedGroup = value;
                //_selectedGroup.NewInstanceAdded += OnNewInstanceAddedToGroup;
            }
        }
        /// <summary>
        /// Группы сборок
        /// </summary>
        public IReadOnlyCollection<InstancesGroup> InstancesGroups { get => _groups; }


        #endregion Properties


        #region Constructors


        public LibraryController(AppCore appCore, ClientsManager clientsManager, Action<InstanceClient> export, Action<InstanceModelBase> setRunningGame, 
            Func<InstanceClient, InstanceModelBase> getInstanceModelByInstanceClient, Action<InstanceModelBase> addInstanceModel)
        {
            _appCore = appCore;
            _clientsManager = clientsManager;
            _groups = new(_clientsManager.GetExistsGroups());
            _exportFunc = export;
            _setRunningGame = setRunningGame;
            
            _getInstanceModelByInstanceClient = getInstanceModelByInstanceClient;
            _addInstanceModel = addInstanceModel;

            InstanceModelBase.GlobalAddedToLibrary += (im) => Add(im);
            InstanceModelBase.GlobalDeletedEvent += Remove;
            InstanceModelBase.GlobalGroupRemovedEvent += Remove;
        }


        #endregion Constructors


        #region Public Methods


        public void Add(InstanceModelBase instanceModelBase, [CallerMemberName] string member = "")
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Runtime.DebugWrite(instanceModelBase.Id, color: ConsoleColor.Red);
                _instances.Add(instanceModelBase);
                InstanceAdded?.Invoke(instanceModelBase);
                OnPropertyChanged(nameof(Instances));
            });
        }

        /// <summary>
        /// Добавляет сборку используется только при раздаче.
        /// Может получать instanceclient для которого существует уже созданных экземлпяр instancemodel,
        /// что будет создавать дубликаты например в уведомлениях.
        /// </summary>
        public InstanceModelBase? Add(InstanceClient instanceClient, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = _getInstanceModelByInstanceClient(instanceClient);

            if (instanceModelBase != null) 
            {
                instanceModelBase.UpdateInstancesGroup(SelectedGroup);
            }

            foreach (var i in _instances)
            {
                Runtime.DebugWrite($"{instanceClient.Name} {i.InstanceClient.Name}\t|\t{instanceClient.ExternalId} {i.InstanceClient.ExternalId}\t|\t{i.InstanceClient == instanceClient}");
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                if (instanceModelBase == null)
                {
                    var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame, addToLibrary: (ic, id) => Add(ic, id), group: SelectedGroup);
                    instanceModelBase = new InstanceModelBase(args);
                    _addInstanceModel(instanceModelBase);
                }
                Add(instanceModelBase);
            });

            return instanceModelBase;
        }

        /// <summary>
        /// Добавляет сборку используется только при раздаче.
        /// Следовательно, добавляется всегда единственный экземпляр без существующих копий
        /// </summary>
        public InstanceModelBase? Add(InstanceClient instanceClient, InstanceDistribution instanceDistribution, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame, instanceDistribution, addToLibrary: (ic, id) => Add(ic, id), group: SelectedGroup);
                instanceModelBase = new InstanceModelBase(args);
                Add(instanceModelBase);
            });

            return instanceModelBase;
        }

        /// <summary>
        /// Добавляет сборку используется только в случае наличия ImportData (импорт, копирование).
        /// Следовательно, добавляется всегда единственный экземпляр без существующих копий
        /// </summary>
        public InstanceModelBase? Add(InstanceClient instanceClient, ImportData? importData)
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame, addToLibrary: (ic, id) => Add(ic, id), importData: importData, group: SelectedGroup);
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


        public InstanceModelBase? Get(InstanceClient instanceClient)
        {
            if (instanceClient == null)
                return null;

            return _instances.FirstOrDefault(i => i.Equals(instanceClient));
        }

        public void GroupItemsChanged(InstanceClient client)
        {
            Add(client);
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

            _instances.Clear();

            foreach (var ic in SelectedGroup.Clients)
            {
                var im = _getInstanceModelByInstanceClient(ic); 
                if (im == null)
                {
                    Add(ic);
                }
                else
                {
                    im.UpdateInstancesGroup(instancesGroup);
                    Add(im);
                }
            }

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
