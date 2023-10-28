using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceCatalogControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.MainContent
{
    public sealed class LibraryModel : ViewModelBase
    {
        public IEnumerable<InstanceModelBase> Instances { get => LibraryController.Instance.Instances; }


        #region Constructors


        public LibraryModel()
        {

        }


        #endregion Constructors
    }
}
