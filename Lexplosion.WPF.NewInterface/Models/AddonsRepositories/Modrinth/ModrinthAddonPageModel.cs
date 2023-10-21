using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Core;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Models.AddonsRepositories
{
    public sealed class ModrinthAddonPageModel : ViewModelBase
    {
        private readonly InstanceAddon _instanceAddon;


        #region Properties


        public byte[] ImageBytes { get => _instanceAddon.Logo; }

        public string Name { get => _instanceAddon.Name; }
        public string Summary { get => _instanceAddon.Description; }
        public IEnumerable<CategoryBase> Categories { get => new List<CategoryBase>(); }


        #endregion Proeprties


        #region Constructors


        public ModrinthAddonPageModel(InstanceAddon instanceAddon)
        {
            _instanceAddon = instanceAddon;
        }


        #endregion Constructors
    }

}
