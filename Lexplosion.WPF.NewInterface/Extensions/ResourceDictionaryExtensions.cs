using System.Windows;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    internal static class ResourceDictionaryExtensions
    {
        public static bool TryGetValue<T, U>(this ResourceDictionary dictionary, T key, out U result)
        {
            try
            {

                foreach (T _key in dictionary.Keys)
                {
                    if (_key.Equals(key))
                    {
                        result = (U)dictionary[_key];
                        return true;
                    }
                }
            }
            catch
            {
            }

            result = default(U);
            return false;
        }
    }
}
