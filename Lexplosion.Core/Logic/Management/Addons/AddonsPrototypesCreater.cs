using System.Collections.Generic;
using System.IO;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management.Addons
{
    static class AddonsPrototypesCreater
    {
        public static IPrototypeAddon CreateFromFile(BaseInstanceData indtanceData, string filePath)
        {
            string md5;
            string sha1;
            long fileLenght;

            using (FileStream stream = File.OpenRead(filePath))
            {
                string sha512 = Cryptography.Sha512(stream);
                stream.Position = 0;
                sha1 = Cryptography.Sha1(stream);
                stream.Position = 0;

                fileLenght = stream.Length;

                // ищем файл на модринфе
                List<ModrinthProjectFile> projectFiles = ModrinthApi.GetFilesFromHash(sha512);
                foreach (var projectFile in projectFiles)
                {
                    if (projectFile?.Files != null && projectFile.Files.Count > 0)
                    {
                        foreach (var file in projectFile.Files)
                        {
                            bool isHash = !string.IsNullOrEmpty(file.Hashes?.Sha512) && !string.IsNullOrEmpty(file.Hashes.Sha1);
                            if (isHash && file.Hashes.Sha512 == sha512 && file.Hashes.Sha1 == sha1 && file.Size == fileLenght)
                            {
                                return new ModrinthAddon(indtanceData, projectFile);
                            }
                        }
                    }
                }

                //если дошли до сюда - значит файл на модринфе не найден. Вычисляем md5 для работы с курсфорджем
                md5 = Cryptography.Md5(stream);
            }

            //на модринфе не нашелся, ищем на курсфордже
            {
                string fingerprint = StupidHash.Compute(File.ReadAllBytes(filePath)).ToString();

                List<CurseforgeFileInfo> projectFiles = CurseforgeApi.GetFilesFromFingerprints(new string[1] { fingerprint });
                foreach (var projectFile in projectFiles)
                {
                    if (projectFile?.hashes != null)
                    {
                        string fileSha1 = null;
                        string fileMd5 = null;

                        foreach (var hash in projectFile.hashes)
                        {
                            if (hash.algo == CurseforgeFileInfo.Hashes.Algorithm.Sha1 && !string.IsNullOrEmpty(hash.value))
                            {
                                fileSha1 = hash.value;
                            }
                            else if (hash.algo == CurseforgeFileInfo.Hashes.Algorithm.Md5 && !string.IsNullOrEmpty(hash.value))
                            {
                                fileMd5 = hash.value;
                            }
                        }

                        if (fileSha1 == sha1 && fileMd5 == md5 && fileLenght == projectFile.fileLength)
                        {
                            return new CurseforgeAddon(indtanceData, projectFile);
                        }
                    }
                }
            }

            return null;
        }

        public static Dictionary<string, IPrototypeAddon> CreateFromFile(BaseInstanceData indtanceData, List<string> files)
        {
            return null;
        }
    }
}
