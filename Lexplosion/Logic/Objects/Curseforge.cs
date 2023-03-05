using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Lexplosion.Logic.Objects.Curseforge
{
    /// <summary>
    /// Описывает проект курсфорджа. Дочерние классы используются при декодировании Json
    /// </summary>
    public abstract class CurseforgeProjectInfo
    {
        public class GameVersion
        {
            public string gameVersion;
            public int fileId;
        }

        public class Author
        {
            public string name;
            public string url;
        }

        public class Attachment
        {
            public int id;
            public bool isDefault;
            public string thumbnailUrl;
            public string url;
        }

        public class Logo
        {
            public string url;
        }

        public class Links
        {
            public string websiteUrl;
            public string wikiUrl;
            public string issuesUrl;
            public string sourceUrl;
        }

        public string id;
        public string name;
        public int? classId;
        public string summary;
        public float downloadCount { get; set; }
        public string dateModified;
        public Links links;
        //public List<Attachment> attachments;
        public List<CurseforgeCategory> categories;
        public List<Author> authors;
        public Logo logo;

        public string GetAuthorName
        {
            get
            {
                return (authors != null && authors.Count > 0) ? authors[0].name : "Unknown author";
            }
        }
    }

    public class CurseforgeCategory : IProjectCategory
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("classId")]
        public string ClassId { get; set; }

        [JsonProperty("parentCategoryId")]
        public string ParentCategoryId { get; set; }
    }

    /// <summary>
    /// Описывает модпак с курсфорджа. Используются при декодировании Json
    /// </summary>
    public class CurseforgeInstanceInfo : CurseforgeProjectInfo
    {
        public class Screenshot
        {
            public string url;
        }

        public class LatestFile
        {
            public long id;
            public List<string> gameVersions;
        }

        public List<LatestFile> latestFiles = null;
        public List<GameVersion> latestFilesIndexes;
        public List<Screenshot> screenshots;

        /// <summary>
        /// Эта хуйня возвращает тип модлоадера. При каждом вызове тип вычисляется заново, поэтому лучше этот геттер несколько раз не вызывать.
        /// </summary>
        public ClientType ModloaderType
        {
            get
            {
                ClientType modloaderType = ClientType.Vanilla;
                if (latestFiles != null && latestFiles.Count > 0)
                {
                    long maxId = latestFiles[0].id;
                    foreach (var value in latestFiles)
                    {
                        if (value.id > maxId || modloaderType == ClientType.Vanilla)
                        {
                            if (value.gameVersions != null)
                            {
                                if (value.gameVersions.Contains("Forge"))
                                {
                                    modloaderType = ClientType.Forge;
                                }
                                else if (value.gameVersions.Contains("Fabric"))
                                {
                                    modloaderType = ClientType.Fabric;
                                }
                                else if (value.gameVersions.Contains("Quilt"))
                                {
                                    modloaderType = ClientType.Quilt;
                                }
                            }

                            if (value.id > maxId)
                            {
                                maxId = value.id;
                            }
                        }
                    }
                }

                return modloaderType;
            }
        }
    }

    /// <summary>
    /// Описывает мод с курсфорджа. Используются при декодировании Json
    /// </summary>
    public class CurseforgeAddonInfo : CurseforgeProjectInfo
    {
        public class GameVersionAddon : GameVersion
        {
            public int? modLoader;
            public string filename;
        }

        public List<CurseforgeFileInfo> latestFiles;
        public List<GameVersionAddon> latestFilesIndexes;
    }

    public class CurseforgeFileInfo
    {
        public int id;
        public string modId;
        public string fileName;
        public string displayName;
        public List<Dictionary<string, string>> dependencies;
        public List<string> gameVersions;
        public string fileDate;
        public int releaseType;

        // т.к разрабы курсфорджа дефектные рукожопы и конченные недоумки, которые не умеют писать код, то url иногда может быть null, поэтому придётся мутить костыли
        private string _downloadUrl;
        public string downloadUrl
        {
            set
            {
                _downloadUrl = value;
            }
            get
            {
                if (!String.IsNullOrWhiteSpace(_downloadUrl))
                {
                    return _downloadUrl;
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(fileName))
                    {
                        // ручками формируем url
                        return "https://edge.forgecdn.net/files/" + (id / 1000) + "/" + (id % 1000) + "/" + fileName;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    public class InstanceManifest
    {
        public class McVersionInfo
        {
            public string version;
            public List<ModLoaders> modLoaders;
        }

        public class ModLoaders
        {
            public string id;
            public bool primary;
        }

        public class FileData
        {
            public string projectID;
            public string fileID;
        }

        public McVersionInfo minecraft;
        public string name;
        public string version;
        public string author;
        public List<FileData> files;
    }
}
