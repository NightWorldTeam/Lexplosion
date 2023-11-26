using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceCatalogControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
{
    public sealed class LibraryModel : ViewModelBase
    {
        private readonly Func<IEnumerable<InstanceModelBase>> _getInstances;

        public IEnumerable<InstanceModelBase> Instances { get => _getInstances(); }


        #region Constructors


        public LibraryModel(Func<IEnumerable<InstanceModelBase>> getInstances)
        {
            _getInstances = getInstances;
        }


        #endregion Constructors
    }
}
