namespace Lexplosion.UI.WPF.Mvvm.Models.Profile
{
    public readonly struct ProfileTitle
    {
        public ProfileTitle(string name, uint color)
        {
            Name = name;
            Color = color;
        }

        public string Name { get; }
        public uint Color { get; } = 0x167FFC;
        public uint BackgroundColor { get; } = 0x0;
        public uint BorderColor { get; } = 0x66167FFC;
    }
}
