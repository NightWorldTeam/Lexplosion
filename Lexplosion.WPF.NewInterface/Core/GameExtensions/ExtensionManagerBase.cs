using Lexplosion.Logic;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Tools;
using Lexplosion.WPF.NewInterface.Core.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lexplosion.WPF.NewInterface.Core.GameExtensions
{
    /// <summary>
    /// Загружает, хранит версии модлоадера.
    /// </summary>
    public abstract class ExtensionManagerBase : ObservableObject
    {
        /// <summary>
        /// GameExtension (Fabric, Optifine, etc)
        /// ConcurrentDictionary<GameVersion, MinecraftExtension>
        /// </summary>
        private static readonly IDictionary<GameExtension, ConcurrentDictionary<MinecraftVersion, MinecraftExtension>> _extensionVersions;
        /// <summary>
        /// Выбранное расшерение.
        /// </summary>
        public MinecraftExtension CurrentMinecraftExtension { get; private set; }

        public event Action<MinecraftExtension> MinecraftExtensionLoaded;


        #region Constructors


        static ExtensionManagerBase()
        {
            _extensionVersions = new Dictionary<GameExtension, ConcurrentDictionary<MinecraftVersion, MinecraftExtension>>();

            // заполняем словарь всеми возможными типами расширения.
            foreach (GameExtension value in Enum.GetValues(typeof(GameExtension)))
            {
                _extensionVersions.Add(value, new ConcurrentDictionary<MinecraftVersion, MinecraftExtension>());
            }
        }

        protected ExtensionManagerBase(GameExtension gameExtension, MinecraftVersion minecraftVersion, bool isEnable = true)
        {
            CurrentMinecraftExtension = new MinecraftExtension(new ReadOnlyCollection<string>(new List<string>()), gameExtension);
            if (MinecraftExtension.CheckExistsOnVersion(minecraftVersion, gameExtension))
            {
                foreach (var enumValue in Enum.GetValues(typeof(GameExtension)))
                {
                    if (gameExtension == (GameExtension)enumValue)
                    {
                        // load extension versions
                        Lexplosion.Runtime.TaskRun(() =>
                        {
                            CurrentMinecraftExtension = LoadExtensionVersions(gameExtension, minecraftVersion);
                            OnPropertyChanged(nameof(CurrentMinecraftExtension));
                            MinecraftExtensionLoaded?.Invoke(CurrentMinecraftExtension);
                            OnPropertiesChanged();
                        });
                        // load extensions versions and select first version
                    }
                    else
                    {
                        Lexplosion.Runtime.TaskRun(() =>
                        {
                            LoadExtensionVersions((GameExtension)enumValue, minecraftVersion);
                            OnPropertiesChanged();
                        });
                    }
                }
            }

            OnPropertiesChanged();
        }


        #endregion Constructors


        #region Public & Protected Methods


        /// <summary>
        /// Вызывает метод OnPropertyChanged() свойствам для которых это требуется. 
        /// </summary>
        protected abstract void OnPropertiesChanged();

        private static KeySemaphore<GameExtension> _loadExtensionVersionsSync = new();


		/// <summary>
		/// Возвращает класс для расширения майнкрафта.
		/// Также сохраняет класс в словарь.
		/// </summary>
		/// <param name="extension">Расширение игры (optifine, fabric, etc)</param>
		/// <param name="gameVersion">Версия майнкрафта</param>
		/// <returns>MinecraftExtension</returns>
		public static MinecraftExtension LoadExtensionVersions(GameExtension extension, MinecraftVersion minecraftVersion)
        {
            _loadExtensionVersionsSync.WaitOne(extension);

            try
            {
				// Todo: optimize code, do not save empty value, just return constant for it.

				// if current game version is not contains extension versions 
				if (!_extensionVersions[extension].ContainsKey(minecraftVersion))
				{
					IList<string> extensionVersion;

					// if optifine
					if (GameExtension.Optifine == extension)
						extensionVersion = Runtime.ServicesContainer.MinecraftService.GetOptifineVersions(minecraftVersion.Id);
					else
						extensionVersion = Runtime.ServicesContainer.MinecraftService.GetModloadersList(minecraftVersion.Id, (ClientType)extension);

					_extensionVersions[extension].TryAdd(minecraftVersion, new MinecraftExtension(new ReadOnlyCollection<string>(extensionVersion), extension));
				}
				return _extensionVersions[extension][minecraftVersion];
			}
            finally
            {
				_loadExtensionVersionsSync.Release(extension);
			}			
		}

        /// <summary>
        /// Загружены ли версии расширения для данной версии игры.
        /// </summary>
        /// <param name="gameExtension">Тип расширения</param>
        /// <param name="minecraftVersion">Версия игры</param>
        /// <returns>True/False</returns>
        public static bool IsExtensionLoaded(GameExtension gameExtension, MinecraftVersion minecraftVersion)
        {
            return _extensionVersions[gameExtension].ContainsKey(minecraftVersion) ?
                _extensionVersions[gameExtension][minecraftVersion].IsAvaliable : false;
        }


        #endregion Public & Protected Methods
    }
}