using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Core.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public sealed class LibraryController : IInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;


        private readonly AppCore _appCore;
        private readonly Action<InstanceClient> _exportFunc;
        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();

        #region Properties


        public IReadOnlyCollection<InstanceModelBase> Instances { get => _instances; }


        #endregion Properties


        #region Constructors


        public LibraryController(AppCore appCore, Action<InstanceClient> export)
        {
            _appCore = appCore;

            InstanceModelBase.GlobalAddedToLibrary += (str) => Add(str);
            InstanceModelBase.GlobalDeletedEvent += Remove;

            _exportFunc = export;

            foreach (var ic in InstanceClient.GetInstalledInstances()) 
            {
                Add(ic);
            }
        }


        #endregion Constructors


        #region Public Methods


        public void Add(InstanceModelBase instanceModelBase, [CallerMemberName] string member = "")
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _instances.Add(instanceModelBase);
                InstanceAdded?.Invoke(instanceModelBase);
            });
        }

        public InstanceModelBase? Add(InstanceClient instanceClient, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                instanceModelBase = new InstanceModelBase(_appCore, instanceClient, _exportFunc);
                Add(instanceModelBase);
            });

            return instanceModelBase;
        }

        public InstanceModelBase? Add(InstanceClient instanceClient, InstanceDistribution instanceDistribution, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                instanceModelBase = new InstanceModelBase(_appCore, instanceClient, _exportFunc, instanceDistribution);
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


        #endregion Public Methods
    }
}
