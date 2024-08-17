using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.WPF.NewInterface.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Core.Objects
{
    public class InstanceGroup
    {
        public string Name { get; }
        public string Author { get; } = "by _Hel2x_";
        public int Count { get; }
        public string ImageUrl { get; } = "pack://Application:,,,/Assets/Images/background/authBG.png";
        public IEnumerable<InstanceModelBase> Instances { get; }


        public InstanceGroup(string name, IEnumerable<InstanceModelBase> _instances)
        {
            Name = name;
            Count = _instances.Count();
            Instances = _instances;
        }
    }
}
