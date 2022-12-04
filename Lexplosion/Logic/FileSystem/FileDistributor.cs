using System.Collections.Generic;
using Lexplosion.Logic.Network;
using Lexplosion.Global;
using System.IO;

namespace Lexplosion.Logic.FileSystem
{
    public class FileDistributor
    {
        private static DataServer _dataServer = null;

        private string _fileId;

        private FileDistributor(string fileId)
        {
            _fileId = fileId;
        }

        public static FileDistributor CreateDistribution(string filename)
        {
            if (_dataServer == null)
            {
                _dataServer = new DataServer(GlobalData.User.UUID, GlobalData.User.SessionToken, false, LaunсherSettings.ServerIp);
            }

            //Получаем хэш файла
            string hash;
            using (FileStream fstream = File.OpenRead(filename))
            {
                byte[] fileBytes = new byte[fstream.Length];
                hash = Сryptography.Sha256(fileBytes);
            }

            string answer = ToServer.HttpPost(LaunсherSettings.URL.LogicScripts + "setFileDistribution", new Dictionary<string, string>
            {
                ["UUID"] = GlobalData.User.UUID,
                ["sessionToken"] = GlobalData.User.SessionToken,
                ["fileId"] = hash
            });

            Runtime.DebugWrite(answer);

            _dataServer.AddFile(filename, hash);

            return new FileDistributor(hash);
        }
    }
}
