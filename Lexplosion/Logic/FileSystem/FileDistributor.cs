using System.Collections.Generic;
using Lexplosion.Logic.Network;
using Lexplosion.Global;
using System.IO;
using System.Security.Cryptography;
using System;
using Lexplosion.Tools;
using Lexplosion.Logic.Objects;
using Newtonsoft.Json;

namespace Lexplosion.Logic.FileSystem
{
    public class FileDistributor
    {
        private static DataServer _dataServer = null;
        private static string _publicRsaKey;
        private static string _confirmWord;

        private string _fileId;

        private FileDistributor(string fileId)
        {
            _fileId = fileId;
        }

        public static FileDistributor CreateDistribution(string filename)
        {
            if (_dataServer == null)
            {
                Сryptography.CreateRsaKeys(out RSAParameters privateKey, out _publicRsaKey);
                _confirmWord = new Random().GenerateString(32);

                _dataServer = new DataServer(privateKey, _confirmWord, GlobalData.User.UUID, GlobalData.User.SessionToken, LaunсherSettings.ServerIp);
            }

            //Получаем хэш файла
            string hash;
            using (FileStream fstream = File.OpenRead(filename))
            {
                hash = Сryptography.Sha256(fstream);
            }

            string answer = ToServer.HttpPost(LaunсherSettings.URL.UserApi + "setFileDistribution", new Dictionary<string, string>
            {
                ["UUID"] = GlobalData.User.UUID,
                ["sessionToken"] = GlobalData.User.SessionToken,
                ["FileId"] = hash,
                ["Parameters"] = JsonConvert.SerializeObject(new DistributionData
                {
                    Name = "XYI",
                    PublicRsaKey = _publicRsaKey,
                    ConfirmWord = _confirmWord
                })
            });

            Runtime.DebugWrite(answer);

            _dataServer.AddFile(filename, hash);

            return new FileDistributor(hash);
        }
    }
}
