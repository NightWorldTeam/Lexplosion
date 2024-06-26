namespace Lexplosion.WPF.NewInterface.Core.ViewModel
{
    public interface ILimitedAccess
    {
        /// <summary>
        /// Есть ли доступ к странице.
        /// </summary>
        bool HasAccess { get; }
        /// <summary>
        /// Обновляет данные доступа.
        /// </summary>
        void RefreshAccessData();
    }
}
