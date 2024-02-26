using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public sealed class CatalogController : IInstanceController
    {
        private ObservableCollection<InstanceModelBase> _instances = new ObservableCollection<InstanceModelBase>();
        public IReadOnlyCollection<InstanceModelBase> Instances { get => _instances; }


        private Action<InstanceClient> _exportFunc;


        #region Constructors


        public CatalogController(Action<InstanceClient> exportFunc)
        {
            InstanceModelBase.GlobalAddedToLibrary += Add;
            InstanceModelBase.GlobalDeletedEvent += Remove;

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
                _instances.Add(new InstanceModelBase(instanceClient, _exportFunc));
            });
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
