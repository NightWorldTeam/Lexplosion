using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Network.Web;
using Lexplosion.Logic.Objects.Modrinth;
using Lexplosion.Logic.Management.Instances;

namespace Lexplosion.Logic.Management.Addons
{
    static class AddonsPrototypesCreater
    {
        public static IPrototypeAddon CreateFromFile(BaseInstanceData indtanceData, string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                string sha512 = Cryptography.Sha512(stream);
                stream.Position = 0;
                string sha1 = Cryptography.Sha1(stream);
                List<ModrinthProjectFile> projectFiles = ModrinthApi.GetFilesFromHash(sha512);

                foreach (var projectFile in projectFiles)
                {
                    if (projectFile.Files != null && projectFile.Files.Count > 0)
                    {
                        foreach (var file in projectFile.Files)
                        {
                            bool isHash = !string.IsNullOrEmpty(file.Hashes?.Sha512) && !string.IsNullOrEmpty(file.Hashes.Sha1);
                            if (isHash && file.Hashes.Sha512 == sha512 && file.Hashes.Sha1 == sha1 && file.Size == stream.Length)
                            {
                                return new ModrinthAddon(indtanceData, projectFile);
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
