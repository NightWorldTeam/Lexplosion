using Lexplosion.Gui.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Lexplosion.Gui.Models
{
    public class MainModel : VMBase
    { 
        public static ObservableCollection<InstanceFormViewModel> AddedInstanceForms { get; } = new ObservableCollection<InstanceFormViewModel>();

        public static List<string> GetOutsideIds() 
        {
            List<string> OutsideIds = new List<string>();
            foreach (var aif in AddedInstanceForms)
            {
                if (aif.Model.Instance.OutsideId != null || aif.Model.Instance.OutsideId != "")
                    OutsideIds.Add(aif.Model.Instance.LocalId);
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
