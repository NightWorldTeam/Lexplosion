namespace Lexplosion.Logic.FileSystem.StorageManagment
{
    public interface IDataHandlerArgs<T>
    {
        public IDataHandler<T> Handler { get; }
    }
}
