using System;
using System.Threading;

namespace Lexplosion.Tools
{

    /// <summary>
    /// Аргументы для как-йто долгой задачи.
    /// </summary>
    public struct TaskArgs
    {
        /// <summary>
        /// Токен отмены
        /// </summary>
        public CancellationToken CancelToken;
        /// <summary>
        /// обработчки процентов.
        /// </summary>
        public Action<int> PercentHandler;
    }
}
