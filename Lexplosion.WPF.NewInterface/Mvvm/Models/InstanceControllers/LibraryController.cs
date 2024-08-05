using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core.Notifications;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public sealed class LibraryController : IInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;


        private readonly Action<InstanceClient> _exportFunc;
        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();

        #region Properties

        public NotifyCallback Notify { get; }
        public IReadOnlyCollection<InstanceModelBase> Instances { get => _instances; }


        #endregion Properties


        #region Constructors


        public LibraryController(Action<InstanceClient> export, NotifyCallback? notify = null)
        {
            Notify = notify;

            InstanceModelBase.GlobalAddedToLibrary += Add;
            InstanceModelBase.GlobalDeletedEvent += Remove;

            _exportFunc = export;

            foreach (var ic in InstanceClient.GetInstalledInstances()) 
            {
                Add(ic);
            }
        }


        #endregion Constructors


        #region Public Methods


        public void Add(InstanceModelBase instanceModelBase)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _instances.Add(instanceModelBase);
                InstanceAdded?.Invoke(instanceModelBase);
            });
        }

        public void Add(InstanceClient instanceClient)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Add(new InstanceModelBase(instanceClient, _exportFunc, Notify));
            });
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


        #endregion Public Methods
    }
}
