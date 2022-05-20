using Lexplosion.Gui.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.Gui.Models
{
    public class MainModel : VMBase
    { 
        public static ObservableCollection<InstanceFormViewModel> LibraryInstances { get; } = new ObservableCollection<InstanceFormViewModel>();

        //public static List<string> GetOutsideIds() 
        //{
        //    var OutsideIds = new List<string>();
        //    foreach (var aif in LibraryInstances.ToArray())
        //    {
        //        if (aif.Model.InstanceClient.OutsideId != null || aif.Model.InstanceClient.OutsideId != "")
        //            OutsideIds.Add(aif.Model.InstanceClient.OutsideId);
        //    }

        //    return OutsideIds;
        //}

        //public static InstanceFormViewModel GetSpecificVM(string outsideId) 
        //{
        //    foreach (var vms in LibraryInstances) 
        //    {
        //        if (vms.Model.InstanceClient.OutsideId == outsideId)
        //            return vms;
        //    }
        //    return null;
        //}
    }
}
