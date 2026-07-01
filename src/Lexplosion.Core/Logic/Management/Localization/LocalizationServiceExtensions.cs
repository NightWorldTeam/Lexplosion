 namespace Lexplosion.Logic.Management.Localization
{
    /// <summary>
    /// Extension methods для <see cref="ILocalizationService"/> предоставляющие типизированный доступ к локализованным строкам
    /// <para>
    /// Когда добавляете чето в <see cref="LocalizationKeys"/>, добавьте еще и сюда
    /// </para>
    /// </summary>
    public static class LocalizationServiceExtensions
    {
        public static string GetNoDescription(this ILocalizationService service)
            => service.GetString(LocalizationKeys.NoDescription);
        public static string GetUnknownName(this ILocalizationService service)
            => service.GetString(LocalizationKeys.UnknownName);
        public static string GetUnknownAuthor(this ILocalizationService service)
            => service.GetString(LocalizationKeys.UnknownAuthor);
        public static string GetLaunchingGame(this ILocalizationService service)
            => service.GetString(LocalizationKeys.LaunchingGame);
    }
}
