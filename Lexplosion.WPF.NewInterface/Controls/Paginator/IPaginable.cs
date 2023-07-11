namespace Lexplosion.WPF.NewInterface.Controls.Paginator
{
    public interface IPaginable
    {
        public bool IsEmptyPage { get; set; }
        public bool IsLastPage { get; set; }
        public uint ItemsPerPage { get; set; }
        public void Paginate(uint scrollTo);
    }
}
