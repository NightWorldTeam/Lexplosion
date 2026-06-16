namespace Lexplosion.Logic.Management.Localization
{
	/// <summary>
	/// Предоставляет доступ к сервису локализации
	/// </summary>
	public interface ILocalizationServicesContainer
	{
		ILocalizationService LocalizationService { get; }
	}
}
