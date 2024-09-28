using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lexplosion.Global;
using Lexplosion.Tools;

namespace Lexplosion.Logic
{
    [Serializable]
    public class Settings
    {
        private string _javaPath = null;
        public string JavaPath
        {
            get
            {
                return (IsCustomJava == true) ? _javaPath : "";
            }
            set
            {
                _javaPath = value;
            }
        }

        private string _java17Path = null;
        public string Java17Path
        {
            get
            {
                return (IsCustomJava17 == true) ? _java17Path : "";
            }
            set
            {
                _java17Path = value;
            }
        }

        public bool? IsCustomJava = null;
        public bool? IsCustomJava17 = null;
        public string GamePath = null;
        public uint? Xmx = null;
        public uint? Xms = null;
        public uint? WindowWidth = null;
        public uint? WindowHeight = null;
        public bool? IsShowConsole = null;
        public bool? IsHiddenMode = null;
        public bool? IsAutoUpdate = null;
        public bool? NwClientByDefault = null;
        public string GameArgs = null;
        /// <summary>
        /// Использовать ли в приоритете в сетевой игре прямое подключение.
        /// </summary>
        public bool NetworkDirectConnection = true;
        public string LanguageId = "";
        public string AccentColor = "";
        public string ThemeName = "";
        /// <summary>
        /// Адрес сервера для автовхода при запуске майкрафта.
        /// Параметр только для настроек конкретного клиента. 
        /// </summary>
        public string AutoLoginServer = null;

        /// <summary>
        /// Выполняет слияние с другим экземпляром настроек.
        /// Если режим приоритетный, то в данном экземпляре будут заменены только пустые поля на поля из settings,
        /// Если режим не приоритетный, то в дааном экземпляре будут заменены все поля, которые в settings не пустые.
        /// </summary>
        /// <param name="settings">Экземпляр, с которым нужно выполнить слияние</param>
        /// <param name="priority">Приоритетный ли режим</param>
        public void Merge(Settings settings, bool priority = false)
        {
            if (priority)
            {
                if (string.IsNullOrWhiteSpace(JavaPath)) JavaPath = settings.JavaPath;
                if (string.IsNullOrWhiteSpace(Java17Path)) Java17Path = settings.Java17Path;
                if (IsCustomJava == null) IsCustomJava = settings.IsCustomJava;
                if (IsCustomJava17 == null) IsCustomJava17 = settings.IsCustomJava17;
                if (GamePath == null) GamePath = settings.GamePath;
                if (Xmx == null) Xmx = settings.Xmx;
                if (Xms == null) Xms = settings.Xms;
                if (WindowWidth == null) WindowWidth = settings.WindowWidth;
                if (WindowHeight == null) WindowHeight = settings.WindowHeight;
                if (IsShowConsole == null) IsShowConsole = settings.IsShowConsole;
                if (IsHiddenMode == null) IsHiddenMode = settings.IsHiddenMode;
                if (IsAutoUpdate == null) IsAutoUpdate = settings.IsAutoUpdate;
                if (NwClientByDefault == null) NwClientByDefault = settings.NwClientByDefault;
                if (GameArgs == null) GameArgs = settings.GameArgs;
                if (string.IsNullOrWhiteSpace(AutoLoginServer)) AutoLoginServer = settings.AutoLoginServer;
                if (string.IsNullOrWhiteSpace(LanguageId)) LanguageId = settings.LanguageId;
                if (string.IsNullOrWhiteSpace(ThemeName)) ThemeName = settings.ThemeName;
                if (string.IsNullOrWhiteSpace(AccentColor)) AccentColor = settings.AccentColor;
            }
            else
            {
                if (settings.JavaPath != null) JavaPath = settings.JavaPath;
                if (settings.Java17Path != null) Java17Path = settings.Java17Path;
                if (settings.IsCustomJava != null) IsCustomJava = settings.IsCustomJava;
                if (settings.IsCustomJava17 != null) IsCustomJava17 = settings.IsCustomJava17;
                if (settings.GamePath != null) GamePath = settings.GamePath;
                if (settings.Xmx != null) Xmx = settings.Xmx;
                if (settings.Xms != null) Xms = settings.Xms;
                if (settings.WindowWidth != null) WindowWidth = settings.WindowWidth;
                if (settings.WindowHeight != null) WindowHeight = settings.WindowHeight;
                if (settings.IsShowConsole != null) IsShowConsole = settings.IsShowConsole;
                if (settings.IsHiddenMode != null) IsHiddenMode = settings.IsHiddenMode;
                if (settings.IsAutoUpdate != null) IsAutoUpdate = settings.IsAutoUpdate;
                if (settings.NwClientByDefault != null) NwClientByDefault = settings.NwClientByDefault;
                if (settings.GameArgs != null) GameArgs = settings.GameArgs;
                if (!string.IsNullOrWhiteSpace(settings.LanguageId)) LanguageId = settings.LanguageId;
                if (!string.IsNullOrWhiteSpace(settings.ThemeName)) ThemeName = settings.ThemeName;
                if (!string.IsNullOrWhiteSpace(settings.AccentColor)) AccentColor = settings.AccentColor;
                AutoLoginServer = settings.AutoLoginServer;
            }

            NetworkDirectConnection = settings.NetworkDirectConnection;
        }

        public Settings Copy()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (Settings)formatter.Deserialize(ms);
            }
        }

        public static Settings GetDefault()
        {
            uint xmx = Environment.Is64BitOperatingSystem ? (uint)(NativeMethods.GetRamCount() / 2) : (uint)1024;
            if (xmx > 8192) xmx = 8192;
            if (xmx < 1024) xmx = 1024;

            return new Settings
            {
                JavaPath = "",
                Java17Path = "",
                IsCustomJava = false,
                IsCustomJava17 = false,
                GamePath = LaunсherSettings.gamePath,
                Xmx = xmx,
                Xms = 256,
                WindowWidth = 854,
                WindowHeight = 480,
                IsShowConsole = false,
                IsHiddenMode = false,
                GameArgs = "",
                IsAutoUpdate = false,
                NetworkDirectConnection = true,
                NwClientByDefault = true,
                LanguageId = "ru-RU",
                ThemeName = "DarkColorTheme",
                AccentColor = "#167ffc"
            };
        }
    }
}
