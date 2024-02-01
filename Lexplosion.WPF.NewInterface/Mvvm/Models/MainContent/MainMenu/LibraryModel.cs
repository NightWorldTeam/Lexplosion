using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
{
    public sealed class LibraryModel : ViewModelBase
    {
        private readonly IInstanceController _instanceCatalog;
        public IEnumerable<InstanceModelBase> Instances { get => _instanceCatalog.Instances; }


        #region Constructors


        public LibraryModel(IInstanceController instanceCatalog)
        {
            _instanceCatalog = instanceCatalog;
        }


        #endregion Constructors
    }
}
