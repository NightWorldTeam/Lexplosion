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
    public sealed class CatalogController : IInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;

        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();
        private Action<InstanceClient> _exportFunc;



        #region Properties

        
        public IReadOnlyCollection<InstanceModelBase> Instances { get => _instances; }
        public NotifyCallback? Notify { get; }


        #endregion Properties


        #region Constructors


        public CatalogController(Action<InstanceClient> exportFunc, NotifyCallback? notify = null)
        {
            Notify = notify;

            //InstanceModelBase.GlobalAddedToLibrary += Add;
            // Лол, а зачем удалять из каталога?
            //InstanceModelBase.GlobalDeletedEvent += Remove;

            _exportFunc = exportFunc;
        }


        #endregion Constructors


        #region Public Methods


        public void Add(InstanceModelBase instanceModelBase)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _instances.Add(instanceModelBase);
            });
        }

        public void Add(InstanceClient instanceClient)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _instances.Add(new InstanceModelBase(instanceClient, _exportFunc, Notify));
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
            App.Current.Dispatcher.Invoke(() => { 
                _instances.Clear();
            });
        }


        #endregion Public Methods
    }
}
