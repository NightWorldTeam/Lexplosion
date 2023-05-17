using Newtonsoft.Json;

namespace Lexplosion.Logic.FileSystem.StorageManagment
{
    class JsonDataFile
    {
        public void SaveToFile<T>(T data, string fileName)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(data);

                if (jsonString != null)
                {
                    DataFilesManager.SaveFile(fileName, jsonString);
                }

            }
            catch { }
        }

        public T LoadFromFile<T>(string fileName)
        {
            try
            {
                string fileContent = DataFilesManager.GetFile(fileName);
                if (fileContent == null)
                {
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(fileContent);
            }
            catch
            {
                return default;
            }
        }
    }
}
