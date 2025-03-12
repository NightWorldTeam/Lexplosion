namespace Lexplosion.WPF.NewInterface.Core
{
    public class LeftPanelFieldInfo
    {
        public string Name { get; }
        public string Value { get; }

        public LeftPanelFieldInfo(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
