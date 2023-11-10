using System;
using System.Windows;
using System.Windows.Media;

namespace Lexplosion.Tools
{
    internal static class ResourceGetter
    {
        /// <summary>
        /// Возвращает иконку по ключу.
        /// </summary>
        /// <param name="key">rключ</param>
        /// <returns>Иконку в формате Geometry</returns>
        public static Geometry GetIcon(string key)
        {
            return Geometry.Parse((string)App.Current.Resources[key]);
        }

        public static Color GetColor(string key)
        {
            return (Color)Application.Current.Resources[key];
        }

        /// <summary>
        /// Выдает значение для выбранного языка по ключу.
        /// </summary>
        /// <param name="key">ключевое слово для фразы</param>
        /// <returns>Текст на выброном языке</returns>
        public static string GetString(string key)
        {
            return (string)Application.Current.Resources[key] ?? key;
        }

        public static string GetCurrentLangString(string lang, string key)
        {
            try
            {
                var d = new ResourceDictionary();
                d.Source = new Uri(RuntimeApp.LangPath + "en-US.xaml");
                return (string)d[key];
            }
            catch
            {

            }
            return key;
        }
    }
}
