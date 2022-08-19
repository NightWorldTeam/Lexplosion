using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int id; // TODO: подобные поля потом нужно на long заменить
        public string name;
        public int? classId;
        public string summary;
        public float downloadCount { get; set; }
        public string dateModified;
        public Links links;
        //public List<Attachment> attachments;
        public List<Category> categories;
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
        public ModloaderType ModloaderType
        {
            get
            {
                ModloaderType modloaderType = ModloaderType.None;
                if (latestFiles != null && latestFiles.Count > 0)
                {
                    long maxId = latestFiles[0].id;
                    foreach (var value in latestFiles)
                    {
                        if (value.id > maxId || modloaderType == ModloaderType.None)
                        {
                            if (value.gameVersions != null)
                            {
                                if (value.gameVersions.Contains("Forge"))
                                {
                                    modloaderType = ModloaderType.Forge;
                                }
                                else if (value.gameVersions.Contains("Fabric"))
                                {
                                    modloaderType = ModloaderType.Fabric;
                                }
                                else if (value.gameVersions.Contains("Quilt"))
                                {
                                    modloaderType = ModloaderType.Quilt;
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
        //public ModloaderType Modloader;
    }

    public class CurseforgeFileInfo
    {
        public int id;
        public int modId;
        public string downloadUrl;
        public string fileName;
        public string displayName;
        public List<Dictionary<string, int>> dependencies;
        public List<string> gameVersions;
    }
}
