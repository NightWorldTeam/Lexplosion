namespace Lexplosion.Logic.FileSystem.Services
{
    /// <summary>
    /// Объединяет <see cref="ICurseforgeFileServicesContainer"/>, <see cref="INightWorldFileServicesContainer"/> и <see cref="IModrinthFileServicesContainer"/>
    /// </summary>
    public interface IPlatfromServiceContainer : ICurseforgeFileServicesContainer, IModrinthFileServicesContainer, INightWorldFileServicesContainer
    {
    }
}
