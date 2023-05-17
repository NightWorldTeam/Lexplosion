namespace Lexplosion.Logic.FileSystem.StorageManagment
{
    public interface IDataHandler<T>
    {
        void SaveToStorage(T data);
        T LoadFromStorage();
    }
}
