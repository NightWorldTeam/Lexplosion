using Lexplosion.Gui.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexplosion.Gui.Models
{
    public class MainModel : VMBase
    { 
        public static ObservableCollection<InstanceFormViewModel> AddedInstanceForms { get; } = new ObservableCollection<InstanceFormViewModel>();

        public static List<string> GetOutsideIds() 
        {
            List<string> OutsideIds = new List<string>();
            foreach (var aif in AddedInstanceForms.ToArray())
            {
                Console.WriteLine(aif.Model.Instance.OutsideId);
                Console.WriteLine(aif.Model.Instance.LocalId);
                if (aif.Model.Instance.OutsideId != null || aif.Model.Instance.OutsideId != "")
                    OutsideIds.Add(aif.Model.Instance.OutsideId);
            }

            return OutsideIds;
        }

        public static InstanceFormViewModel GetSpecificVM(string outsideId) 
        {
            foreach (var vms in AddedInstanceForms) 
            {
                if (vms.Model.Instance.OutsideId == outsideId)
                    return vms;
            }
            return null;
        }
    }
}
