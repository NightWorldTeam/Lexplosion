using Lexplosion.Logic.Objects.Curseforge;

namespace Lexplosion.Gui.ViewModels.CurseforgeMarket
{
    public class CfCategory : VMBase
    {
        private readonly CurseforgeCategory _curseforgeCategory;

        #region Properities


        public string Name { get => _curseforgeCategory.name; }
        public int Id { get => _curseforgeCategory.id; }
        public byte[] ImageBytes { get; private set; }
        public bool HasSubcategories { get => CfSubCategories != null; }

        private CfCategory[] _cfSubcategories;
        public CfCategory[] CfSubCategories
        {
            get => _cfSubcategories; set
            {
                _cfSubcategories = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSubcategories));
            }
        }


        #endregion Properities

        #region Constructors

        public CfCategory(CurseforgeCategory curseforgeCategory, CfCategory[] categories = null)
        {
            _curseforgeCategory = curseforgeCategory;
            ImageBytes = null;
            CfSubCategories = categories;
        }

        #endregion Constructors

        public override string ToString()
        {
            return Name;
        }
    }
}
