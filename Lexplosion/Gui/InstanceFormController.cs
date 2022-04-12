using Lexplosion.Gui.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui
{
    //public enum InstanceFormState
    //{
    //    Installed,
    //    Update,
    //    Empty,
    //}

    public class ActivityForm
    {
        public readonly string Id;
        public readonly string LocalId;
        public readonly InstanceForm InstanceForm;
        //public InstanceFormState State;
        public ActivityForm(string id, string localId, InstanceForm instanceForm)
        {
            Id = id;
            LocalId = localId;
            InstanceForm = instanceForm;
        }
    }   

    public class ActivityInstanceForms
    {
        public class Keys
        {
            public string LocalId;
            public string Id;

            public Keys(string id, string localId) 
            {
                Id = id;
                LocalId = localId;
            }
        }

        public Dictionary<string, ActivityForm> OutsideActivityForms = new Dictionary<string, ActivityForm>();
        public Dictionary<string, ActivityForm> LocalActivityForms = new Dictionary<string, ActivityForm>();

        private int count;

        public void Add(string id, string localId, ActivityForm form) 
        {
            if (id == null && localId == null) throw new Exception("id, localId - null");
            if (form == null) throw new Exception("ActivityForm - null");

            if (id == null) LocalActivityForms.Add(localId, form);
            else if (localId == null) OutsideActivityForms.Add(id, form);
        }

        public void Clear() 
        {
            OutsideActivityForms.Clear();
            LocalActivityForms.Clear();
        }

        //public ActivityForm this[string key] 
        //{
        //    get { if () }
        //    set { }
        //}
    }

    public class InstanceFormModel 
    {
    
    }

    public class InstanceFormController
    {
        private readonly 
    }
}
