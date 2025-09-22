namespace Lexplosion.WPF.NewInterface.Core
{
    public interface ILayoutViewModel
    {
        public ViewModelBase Content { get; }
        /// <summary>
        /// Обновляет контент который может потребоваться обновить. 
        /// </summary>
        //public void Refresh();
    }
}
