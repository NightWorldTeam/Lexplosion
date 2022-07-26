using Lexplosion.Gui.Extension;
using Lexplosion.Gui.ViewModels;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.Gui.Models
{
    public class MainModel : VMBase
    { 
        public static ObservableDictionary<InstanceClient, InstanceFormViewModel> LibraryInstances { get; } 
            = new ObservableDictionary<InstanceClient, InstanceFormViewModel>();
    }
}
