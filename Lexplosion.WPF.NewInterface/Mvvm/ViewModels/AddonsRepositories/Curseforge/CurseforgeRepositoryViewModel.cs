using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Core;
using Lexplosion.WPF.NewInterface.Mvvm.Models.AddonsRepositories;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.AddonsRepositories
{
    public sealed class CurseforgeRepositoryViewModel : ViewModelBase
    {
        public CurseforgeRepositoryModel Model { get; }


        #region Constructors


        public CurseforgeRepositoryViewModel(BaseInstanceData baseInstanceData)
        {
            Model = new CurseforgeRepositoryModel(baseInstanceData);
        }


        #endregion Constructors
    }
}
