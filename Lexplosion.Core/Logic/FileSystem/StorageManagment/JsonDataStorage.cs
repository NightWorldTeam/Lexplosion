using System.Text;
using Newtonsoft.Json;

namespace Lexplosion.Logic.FileSystem.StorageManagment
{
    class JsonDataStorage<T> : IDataHandler<T>
    {
        public byte[] ConvertToStorage(T data)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(data);

                if (jsonString != null)
                {
                    return Encoding.UTF8.GetBytes(jsonString);
                }

            }
            catch { }

            return default;
        }

        public T ConvertFromStorage(byte[] data)
        {
            try
            {
                var jsonString = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                return default;
            }
        }
    }
}
