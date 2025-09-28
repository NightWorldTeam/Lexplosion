namespace Lexplosion.UI.WPF.Mvvm.Models.Profile
{
    public readonly struct ProfileSocialMedia
    {
        public string Url { get; } = "";
        public string LogoName { get; } = "";
        public string Name { get; } = "";

        public ProfileSocialMedia(string name, string url, string logoName)
        {
            Url = url;
            LogoName = logoName;
        }
    }
}
