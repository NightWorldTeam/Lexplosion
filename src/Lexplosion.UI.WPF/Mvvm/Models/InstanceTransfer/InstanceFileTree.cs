using Lexplosion.Logic.Management.Instances;
using Lexplosion.UI.WPF.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Mvvm.Models.InstanceTransfer
{
    public class InstanceFileTree : ViewModelBase
    {
        private readonly InstanceClient _instanceClient;
        /// <summary>
        /// Список хранит в себе загруженные директории [string].
        /// </summary>
        private List<string> LoadedDirectories = new List<string>();


        /// <summary>
        /// Коллекция [Словарь] который обновляется при добавлении в него значений.
        /// </summary>
        private Dictionary<string, PathLevel> _unitsList;
        public Dictionary<string, PathLevel> UnitsList
        {
            get => _unitsList; set
            {
                _unitsList = value;
                OnPropertyChanged();
                ContentPreload(value);
            }
        }


        #region Constructors


        public InstanceFileTree(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;
            UnitsList = instanceClient.GetPathContent();
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Перевыделяет все элементы в UnitsList.
        /// </summary>
        /// <param name="value">значение выделения</param>
        public void ReselectedAllUnits(bool value)
        {
            if (UnitsList != null)
            {
                foreach (var val in UnitsList.Values)
                {
                    val.IsSelected = value;
                }
            }
        }


        /// <summary>
        /// Загружает поддиректорию для текущего PathLevelю
        /// </summary>
        /// <param name="pathLevel"></param>
        public void SubTreeExpand(PathLevel pathLevel) 
        {
            ContentPreload(
                LoadDirContent(pathLevel.FullPath, pathLevel)
                );    
        }


        #endregion Public Methods


        #region Private Methods


        /// <summary>
        /// Загрузка данных для UnitsList, на один шаг вперёд.
        /// </summary>
        /// <param name="subUnits">Словарь UnitsList</param>
        private void ContentPreload(Dictionary<string, PathLevel> subUnits)
        {
            foreach (var i in subUnits)
            {
                LoadDirContent(i.Value.FullPath, i.Value);
            }
        }


        /// <summary>
        /// Загружает контент для директории. Если директория уже загружена, то производиться выход из метода.
        /// </summary>
        /// <param name="dir"></param>
        private Dictionary<string, PathLevel> LoadDirContent(string dir, PathLevel pathLevel)
        {
            Runtime.DebugWrite(dir);
            if (pathLevel.IsFile)
                return pathLevel.UnitsList = new Dictionary<string, PathLevel>();

            if (LoadedDirectories.Contains(dir) || dir == null)
                return pathLevel.UnitsList;

            pathLevel.UnitsList = _instanceClient.GetPathContent(dir);

            if (pathLevel.IsSelected)
                pathLevel.IsSelected = true;

            LoadedDirectories.Add(dir);
            return pathLevel.UnitsList;
        }


        #endregion Private Methods
    }
}
