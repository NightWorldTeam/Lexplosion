using System.Collections.Generic;
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
    public sealed class PathLevel : VMBase
    {
        /// <summary>
        /// Собстна если этот элемент - файл, то значение true, если папка, то false.
        /// </summary>
        public readonly bool IsFile;

        /// <summary>
        /// Название объекта в иерархии.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Содержит полный путь до юнита, начиная от папки модпака.
        /// </summary>
        public string FullPath;

        /// <summary>
        /// Выбран ли элемент.
        /// </summary>
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                _isSelected = value;
                OnPropertyChanged();
                ReselectUnits();
            }
        }

        private Dictionary<string, PathLevel> _unitsList;
        /// <summary>
        /// Используется только если этот элемент является папкой (IsFile равно false) и если AllUnits имеет значение false. 
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

        #region Contructors


        public PathLevel(string name, bool isFile, string fullPath, bool isSelected = false)
        {
            Name = name;
            IsFile = isFile;
            FullPath = fullPath;
            IsSelected = isSelected;
        }


        #endregion Contructors


        #region Private Methods


        private void ReselectUnits()
        {
            if (UnitsList != null)
                foreach (var val in UnitsList.Values)
                {
                    val.IsSelected = this.IsSelected;
                }
        }


        #endregion Private Methods
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
}
