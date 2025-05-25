using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public sealed class CatalogController : IInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;

        private readonly AppCore _appCore;
        private readonly Action<InstanceClient> _exportFunc;
        private readonly Action<InstanceModelBase> _setRunningGame;

        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();



        #region Properties


        public IReadOnlyCollection<InstanceModelBase> Instances { get => _instances; }


        #endregion Properties


        #region Constructors


        public CatalogController(AppCore appCore, Action<InstanceClient> exportFunc, Action<InstanceModelBase> setRunningGame)
        {
            _appCore = appCore;

            //InstanceModelBase.GlobalAddedToLibrary += Add;
            // Лол, а зачем удалять из каталога?
            //InstanceModelBase.GlobalDeletedEvent += Remove;

            _exportFunc = exportFunc;
            _setRunningGame = setRunningGame;
        }


        #endregion Constructors


        #region Public Methods


        public void Add(InstanceModelBase instanceModelBase, [CallerMemberName] string member = "")
        {
            Runtime.DebugWrite($"{member} {instanceModelBase.Name}");
            App.Current.Dispatcher.Invoke(() =>
            {
                _instances.Add(instanceModelBase);
            });
        }

        public InstanceModelBase? Add(InstanceClient instanceClient, [CallerMemberName] string member = "")
        {
            InstanceModelBase? instanceModelBase = null;
            Runtime.DebugWrite($"{member} {instanceClient.Name}");
            App.Current.Dispatcher.Invoke(() =>
            {
                var args = new InstanceModelArgs(_appCore, instanceClient, _exportFunc, _setRunningGame);
                instanceModelBase = new InstanceModelBase(args);
                _instances.Add(instanceModelBase);
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
            App.Current.Dispatcher.Invoke(() => { 
                _instances.Clear();
            });
        }


        #endregion Public Methods
    }
}
