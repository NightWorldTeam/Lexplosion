using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Logic.Objects;

namespace Lexplosion.Logic.Management.Instances
{
    class InstalledInstance
    {
        public string Name;
        public bool IsInstalled;
        public InstanceSource Type;
    }

    /// <summary>
    /// Нужен для экспорта сборки. Содержит описание элемента директории модпака (папки или файла)
    /// </summary>
    public class PathLevel
    {
        /// <summary>
        /// Ипсользуется только если этот элемент является папкой (IsFile равно false).  
        /// Означает что все вложенные файлы и папки должны быть экспортированны из этой папки.
        /// </summary>
        public bool AllUnits = true;
        /// <summary>
        /// Собстна если этот эоемент файл - то значение true, если папка, то false.
        /// </summary>
        public bool IsFile;
        /// <summary>
        /// Ипсользуется только если этот элемент является папкой (IsFile равно false) и если AllUnits имеет значение false. 
        /// Содержит список вложенных элементов, которые должны быть экспортированы из этой папки.
        /// </summary>
        public Dictionary<string, PathLevel> UnitsList;
    }

    /// <summary>
    /// Содержит основную инфу о модпаке.
    /// </summary>
    public class BaseInstanceData
    {
        public InstanceSource Type;
        public string GameVersion;
        public string LocalId;
        public string ExternalId;
        public bool InLibrary;
    }

    /// <summary>
    /// Структура файла, в котором хранятся установленные аддоны (installedAddons.json)
    /// </summary>
    public class InstalledAddons : Dictionary<int, InstalledAddonInfo> { }
}
