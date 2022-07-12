using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexplosion.Gui.Helpers;
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
    public class PathLevel : VMBase
    {
        /// <summary>
        /// Ипсользуется только если этот элемент является папкой (IsFile равно false).  
        /// Означает что все вложенные файлы и папки должны быть экспортированны из этой папки.
        /// </summary>
        public bool AllUnits = true;

        /// <summary>
        /// Ипсользуется только если этот элемент является папкой (IsFile равно false).  
        /// Указывает на то, есть ли в папке файлы.
        /// </summary>
        public bool HasItems
        {
            get => IsFile;
        }

        /// <summary>
        /// Собстна если этот эоемент файл - то значение true, если папка, то false.
        /// </summary>
        public bool IsFile;

        private Dictionary<string, PathLevel> _unitsList;
        /// <summary>
        /// Ипсользуется только если этот элемент является папкой (IsFile равно false) и если AllUnits имеет значение false. 
        /// Содержит список вложенных элементов, которые должны быть экспортированы из этой папки.
        /// </summary>
        public Dictionary<string, PathLevel> UnitsList 
        {
            get => _unitsList; set 
            {
                _unitsList = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Содержит основную инфу о модпаке.
    /// </summary>
    public class BaseInstanceData
    {
        public InstanceSource Type { get; set; }
        public string GameVersion { get; set; }
        public string LocalId { get; set; }
        public string ExternalId { get; set; }
        public bool InLibrary { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public List<Category> Categories { get; set; }
        public string Author { get; set; }
        public ModloaderType Modloader { get; set; }
        public string ModloaderVersion { get; set; }

        public static bool operator ==(BaseInstanceData elem1, BaseInstanceData elem2)
        {
            if (elem1 is null && elem2 is null)
            {
                return (elem1.LocalId == elem2.LocalId);
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(BaseInstanceData elem1, BaseInstanceData elem2)
        {
            return (elem1 is null) || (elem2 is null) || (elem1.LocalId != elem2.LocalId);
        }
    }

    /// <summary>
    /// Структура файла, в котором хранятся установленные аддоны (installedAddons.json)
    /// </summary>
    public class InstalledAddons : Dictionary<long, InstalledAddonInfo> { }
}
