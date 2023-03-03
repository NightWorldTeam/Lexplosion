using Lexplosion.Logic.Network;
using Lexplosion.Tools.Immutable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lexplosion.Gui.Models.GameExtensions
{
    public abstract class ExtensionModel : VMBase, IExtensionModel
    {
        private static readonly Dictionary<GameExtension, ConcurrentDictionary<string, ImmutableArray<string>>> _extensionVersions;

        #region Properties

        public string GameVersion { get; }
        public GameExtension GameExtension { get; set; }

        /// <summary>
        /// Включен ли модлоадер
        /// </summary>
        private bool _isEnable;
        public bool IsEnable
        {
            get => _isEnable; set
            {
                _isEnable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Доступно ли расшерение на эту версию игры.
        /// </summary>
        private bool _isAvaliable;
        public bool IsAvaliable
        {
            get => _isAvaliable; private set
            {
                _isAvaliable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Версия расширения;
        /// </summary>
        private string _version;
        public string Version
        {
            get => _version; set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Массив с версиями расширения
        /// </summary>
        private ImmutableArray<string> _versions;
        public ImmutableArray<string> Versions
        {
            get => _versions; private set
            {
                _versions = value;
                OnPropertyChanged();
            }
        }


        #endregion Properties


        #region Constructors


        static ExtensionModel()
        {
            _extensionVersions = new Dictionary<GameExtension, ConcurrentDictionary<string, ImmutableArray<string>>>()
            {
                { GameExtension.Optifine, new ConcurrentDictionary<string, ImmutableArray<string>>() },
                { GameExtension.Forge, new ConcurrentDictionary<string, ImmutableArray<string>>() },
                { GameExtension.Fabric, new ConcurrentDictionary<string, ImmutableArray<string>>() },
                { GameExtension.Quilt, new ConcurrentDictionary<string, ImmutableArray<string>>() },
            };
        }

        protected ExtensionModel(GameExtension extension, string gameVersion)
        {
            GameVersion = GetCorrectGameVersion(gameVersion);
            GameExtension = extension;

            IsAvaliable = CheckExistsOnVersion(GameVersion, extension);

            Lexplosion.Runtime.TaskRun(() => {
                Versions = LoadExtensionVersions(extension, GameVersion).Result;
                if (Versions.Count > 0)
                {
                    Version = Versions[0];
                    IsAvaliable = true;
                }
                else
                {
                    IsAvaliable = false;
                }
            });
        }


        #endregion Constructors


        #region Private Methods


        public static bool CheckExistsOnVersion(string gameVersion, GameExtension extension) 
        {
            uint[] version = new uint[] { 0, 0, 0 };
            var splitedVersion = gameVersion.Split('.');

            Console.WriteLine(gameVersion);


            switch (extension)
            {
                case GameExtension.Forge: return version[0] >= 1 && version[1] >= 1;
                case GameExtension.Fabric: return version[0] >= 1 && version[1] >= 13;
                case GameExtension.Quilt: return version[0] >= 1 && version[1] >= 14 && version[2] >= 4;
                default: return true;
            }
        }

        public static string GetCorrectGameVersion(string gameVersion) 
        {
            if (gameVersion.Length <= 8) 
            {
                return gameVersion;
            }

            return gameVersion.Replace("snapshot ", "").Replace("release ", "");
        }


        #endregion Private Methods

        #region Public & Protected Methods


        /// <summary>
        /// Возвращает неизменяемый массив с версиями расширениями.
        /// Также сохраняет массив в словарь.
        /// </summary>
        /// <param name="GameExtension"></param>
        /// <param name="gameVersion"></param>
        /// <returns></returns>
        public async static Task<ImmutableArray<string>> LoadExtensionVersions(GameExtension extension, string gameVersion)
        {
            return await Task.Run(() =>
            {
                if (_extensionVersions[extension].ContainsKey(gameVersion))
                    return _extensionVersions[extension][gameVersion];

                if (GameExtension.Optifine == extension)
                {
                    _extensionVersions[extension].TryAdd(gameVersion, new ImmutableArray<string>(ToServer.GetOptifineVersions(gameVersion)));
                }
                else
                {
                    _extensionVersions[extension].TryAdd(gameVersion, new ImmutableArray<string>(ToServer.GetModloadersList(gameVersion, (ClientType)extension)));
                }
                return _extensionVersions[extension][gameVersion];
            });
        }


        #endregion Public & Protected Methods
    }
}
