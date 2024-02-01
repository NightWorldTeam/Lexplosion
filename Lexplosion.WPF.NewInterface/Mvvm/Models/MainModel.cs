using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models
{
    public sealed class MainModel : ViewModelBase
    {
        public IInstanceController CatalogController { get; }
        public IInstanceController LibraryController { get; }

        public MainModel()
        {
            CatalogController = new CatalogController();
            LibraryController = new LibraryController();
        }
    }
}
