namespace Lexplosion.Logic.FileSystem.StorageManagment
{
    public interface IDataHandler<T>
    {
        byte[] ConvertToStorage(T data);
        T ConvertFromStorage(byte[] data);
    }
}
