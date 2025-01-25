using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
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
			Runtime.DebugWrite("CreateFromFile");
            string md5;
            string sha1;
            long fileLenght;
            string sha512;

            try
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    sha512 = Cryptography.Sha512(stream);
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
                                bool isHash = !string.IsNullOrEmpty(file?.Hashes?.Sha512) && !string.IsNullOrEmpty(file.Hashes.Sha1);
                                if (isHash && file.Hashes.Sha512 == sha512 && file.Hashes.Sha1 == sha1 && file.Size == fileLenght)
                                {
									Runtime.DebugWrite("Return modrinth addon");
                                    return new ModrinthAddon(indtanceData, projectFile);
                                }
                            }
                        }
                    }

                    //если дошли до сюда - значит файл на модринфе не найден. Вычисляем md5 для работы с курсфорджем
                    md5 = Cryptography.Md5(stream);
                }
            }
            catch (Exception ex) 
            {
                Runtime.DebugWrite(ex);
                return null;
            }

            //на модринфе не нашелся, ищем на курсфордже
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string fingerprint = StupidHash.Compute(fileBytes).ToString();
                fileBytes = null;

                List<CurseforgeFileInfo> projectFiles = CurseforgeApi.GetFilesFromFingerprints(new List<string> { fingerprint });
                foreach (var projectFile in projectFiles)
                {
                    if (projectFile?.hashes != null)
                    {
                        string fileSha1 = null;
                        string fileMd5 = null;

                        foreach (var hash in projectFile.hashes)
                        {
                            if (hash?.algo == CurseforgeFileInfo.Hashes.Algorithm.Sha1 && !string.IsNullOrEmpty(hash.value))
                            {
                                fileSha1 = hash.value;
                            }
                            else if (hash?.algo == CurseforgeFileInfo.Hashes.Algorithm.Md5 && !string.IsNullOrEmpty(hash.value))
                            {
                                fileMd5 = hash.value;
                            }
                        }

                        if (fileSha1 == sha1 && fileMd5 == md5 && fileLenght == projectFile.fileLength)
                        {
							Runtime.DebugWrite("return curseforge addon");
                            return new CurseforgeAddon(indtanceData, projectFile);
                        }
                    }
                }
            }

            return null;
        }

        public static Dictionary<string, IPrototypeAddon> CreateFromFiles(BaseInstanceData indtanceData, List<string> files)
        {
            var result = new Dictionary<string, IPrototypeAddon>();

            var filesData = new Dictionary<string, SetValues<string, string, long, string>>(); // ключ - путь до файла, знаечния - sha512, sha1, размер файла, md5
            var falesSha512 = new Dictionary<string, string>(); // ключ - sha512, значение - путь до файла
            var hashesToModrinth = new List<string>(); // все sha512

            Runtime.DebugWrite("Start calculate files hashes");

            object locker = new object();
            TasksPerfomer perfomer = new TasksPerfomer(3, files.Count);
            foreach (string file in files)
            {
                perfomer.ExecuteTask(() =>
                {
                    try
                    {
                        using (FileStream stream = File.OpenRead(file))
                        {
                            string sha512 = Cryptography.Sha512(stream);
                            stream.Position = 0;
                            string sha1 = Cryptography.Sha1(stream);
                            stream.Position = 0;
                            string md5 = Cryptography.Md5(stream);

                            lock (locker)
                            {
                                filesData[file] = new SetValues<string, string, long, string>
                                {
                                    Value1 = sha512,
                                    Value2 = sha1,
                                    Value3 = stream.Length,
                                    Value4 = md5
                                };

                                hashesToModrinth.Add(sha512);
                                falesSha512[sha512] = file;
                            }
                        }
                    }
                    catch { }
                });
            }

            perfomer.WaitEnd();
            Runtime.DebugWrite("End calculate files hashes");

            Runtime.DebugWrite("Start search on Modrint");
            Dictionary<string, ModrinthProjectFile> modrinthData = ModrinthApi.GetFilesFromHashes(hashesToModrinth);
            hashesToModrinth = null;

            var hashesToCurseforge = new List<string>();
            var cfHaches = new Dictionary<string, string>(); // ключ - хэш курсфорджа, значение - путь до файла
            var knownProjectFiles = new Dictionary<string, SetValues<string, ModrinthProjectFile>>(); // ключ - айди проекта, значение - путь до файла и CurseforgeFileInfo этого проекта
            foreach (var value in falesSha512)
            {
                string sha512 = value.Key; //sha512
                string filePath = value.Value; // путь до файла

                if (!modrinthData.ContainsKey(sha512)) // в списке  модринфа этого хеша нет. Кладем в список для поиска на курсфордже
                {
                    string fingerprint;
                    try
                    {
                        byte[] allBytes = File.ReadAllBytes(falesSha512[sha512]);
                        fingerprint = StupidHash.Compute(allBytes).ToString();
                    }
                    catch
                    {
                        continue;
                    }

                    hashesToCurseforge.Add(fingerprint);
                    cfHaches[fingerprint] = filePath;
                }
                else // в спсике модринфа этот хэш есть. Проверяем всё ли нормально с этим файлом
                {
                    string sha1 = filesData[filePath].Value2;
                    long fileLenght = filesData[filePath].Value3;

                    // проверяем полученный файл. Сверяем его sha1, размер и еще раз sha512
                    ModrinthProjectFile projectFile = modrinthData[sha512];
                    if (projectFile?.Files != null && projectFile.Files.Count > 0)
                    {
                        foreach (var file in projectFile.Files)
                        {
                            bool isHash = !string.IsNullOrEmpty(file.Hashes?.Sha512) && !string.IsNullOrEmpty(file.Hashes.Sha1);
                            if (isHash && file.Hashes.Sha512 == sha512 && file.Hashes.Sha1 == sha1 && file.Size == fileLenght)
                            {
                                knownProjectFiles[modrinthData[sha512].ProjectId] = new SetValues<string, ModrinthProjectFile>
                                {
                                    Value1 = filePath,
                                    Value2 = modrinthData[sha512]
                                };

                                break;
                            }
                        }
                    }
                }
            }

            modrinthData = null;
            falesSha512 = null;
            GC.Collect();

            List<ModrinthProjectInfo> mdProjects = ModrinthApi.GetProjects(knownProjectFiles.Keys.ToArray());
            foreach (var mdProject in mdProjects)
            {
                if (knownProjectFiles.ContainsKey(mdProject.ProjectId))
                {
                    SetValues<string, ModrinthProjectFile> dataSet = knownProjectFiles[mdProject.ProjectId];
                    result[dataSet.Value1] = new ModrinthAddon(indtanceData, mdProject, dataSet.Value2);
                }
            }

            Runtime.DebugWrite("End search on Modrint");

            if (hashesToCurseforge.Count > 0)
            {
                Runtime.DebugWrite("Start search on Curseforge");

                // разрабы курсфорджа тупорылые недоумки, не умеющие составлять нормальные стрктуры данных,
                // поэтому проходимся по всему полученному списку и формируем новый список cureseforgeData, где string - фингерпринт, а List<CurseforgeFileInfo> - список файлов с этим фингерпринтом
                var cureseforgeData = new Dictionary<string, List<CurseforgeFileInfo>>();
                foreach (var projectFile in CurseforgeApi.GetFilesFromFingerprints(hashesToCurseforge))
                {
                    if (projectFile?.hashes != null && !string.IsNullOrEmpty(projectFile.fileFingerprint))
                    {
                        string fingerprint = projectFile.fileFingerprint;
                        if (!cureseforgeData.ContainsKey(fingerprint))
                        {
                            cureseforgeData[fingerprint] = new List<CurseforgeFileInfo>();
                        }

                        cureseforgeData[fingerprint].Add(projectFile);
                    }
                }

                var knownProjectFiles_ = new Dictionary<string, SetValues<string, CurseforgeFileInfo>>(); // ключ - айди проекта, значение - путь до файла и CurseforgeFileInfo этого проекта
                foreach (var key in cureseforgeData.Keys)
                {
                    foreach (CurseforgeFileInfo projectFile in cureseforgeData[key])
                    {
                        if (cfHaches.ContainsKey(projectFile.fileFingerprint) && filesData.ContainsKey(cfHaches[key]))
                        {
                            string projectFileSha1 = null;
                            string projectFileMd5 = null;

                            SetValues<string, string, long, string> fileData = filesData[cfHaches[key]];
                            string sha1 = fileData.Value2;
                            long fileLenght = fileData.Value3;
                            string md5 = fileData.Value4;

                            foreach (var hash in projectFile.hashes)
                            {
                                if (hash.algo == CurseforgeFileInfo.Hashes.Algorithm.Sha1 && !string.IsNullOrEmpty(hash.value))
                                {
                                    projectFileSha1 = hash.value;
                                }
                                else if (hash.algo == CurseforgeFileInfo.Hashes.Algorithm.Md5 && !string.IsNullOrEmpty(hash.value))
                                {
                                    projectFileMd5 = hash.value;
                                }
                            }

                            if (projectFileSha1 == sha1 && projectFileMd5 == md5 && fileLenght == projectFile.fileLength)
                            {
                                result[cfHaches[key]] = new CurseforgeAddon(indtanceData, projectFile);
                                knownProjectFiles_[projectFile.modId] = new SetValues<string, CurseforgeFileInfo>
                                {
                                    Value1 = cfHaches[key], // путь до файла
                                    Value2 = projectFile
                                };
                            }
                        }
                    }
                }

                List<CurseforgeAddonInfo> cfProjects = CurseforgeApi.GetAddonsInfo(knownProjectFiles_.Keys.ToArray());
                foreach (var cfProject in cfProjects)
                {
                    if (knownProjectFiles_.ContainsKey(cfProject.id))
                    {
                        SetValues<string, CurseforgeFileInfo> dataSet = knownProjectFiles_[cfProject.id];
                        result[dataSet.Value1] = new CurseforgeAddon(indtanceData, cfProject, dataSet.Value2);
                    }
                }

                Runtime.DebugWrite("End search on Curseforge");
            }

            return result;
        }
    }
}
