namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Authorization
{
    public interface IOAuth2Model : IAuthModel
    {
        /// <summary>
        /// Отмена процесса авторизации.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Ручной ввод access token'а.
        /// </summary>
        void ManualInput(string data);
    }
}
