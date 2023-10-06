using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Models.InstanceCatalogControllers;
using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Models.MainContent
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
