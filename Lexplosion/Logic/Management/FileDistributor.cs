using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Network;
using Lexplosion.Global;
using System.IO;
using System.Security.Cryptography;

namespace Lexplosion.Logic.Management
{
    class FileDistributor
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
                _dataServer = new DataServer(GlobalData.User.UUID, GlobalData.User.SessionToken, true, LaunсherSettings.ServerIp);
            }

            //Получаем хэш файла
            byte[] hash;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        hash = sha1.ComputeHash(bs);
                    }
                }
            }

            string base64hash = Convert.ToBase64String(hash);

            // тут сообщеить серверу о поялвнеии файла

            _dataServer.AddFile(filename, base64hash);

            return new FileDistributor(base64hash);
        }
    }
}
