namespace Lexplosion.UI.WPF.Core.Resources.Language
{
    internal class LanguageItem
    {
        public LanguageItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
    }
}
