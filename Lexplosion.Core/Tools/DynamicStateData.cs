using System;

namespace Lexplosion.Tools
{
    /// <summary>
    /// Нужен для передачи информации об изменении какого-то состояния.
    /// </summary>
    /// <typeparam name="TArg">Тип аргумента</typeparam>
    /// <typeparam name="UState">Тип состояния</typeparam>
    public class DynamicStateData<TArg, UState>
    {
        /// <summary>
        /// Возвращает хэндлер, с помощью которого можно вызывать событие StateChanged.
        /// </summary>
        public DynamicStateHandler<TArg, UState> GetHandler
        {
            get
            {
                return new DynamicStateHandler<TArg, UState>(StateChanged);
            }
        }

        /// <summary>
        /// Вызывается когда происходит измнение состояния.
        /// </summary>
        public event Action<TArg, UState> StateChanged;
    }

    /// <summary>
    /// Генерируется классом DynamicStateData и нужен для обновления состояния
    /// </summary>
    /// <typeparam name="TArg">Тип аргумента</typeparam>
    /// <typeparam name="UState">Тип состояния</typeparam>
    public struct DynamicStateHandler<TArg, UState>
    {
        private Action<TArg, UState> _stateChanged;

        public DynamicStateHandler(Action<TArg, UState> stateChangeHandler)
        {
            _stateChanged = stateChangeHandler;
        }

        /// <summary>
        /// Сообщает об изменении состояния.
        /// </summary>
        /// <param name="arg">Аргумент</param>
        /// <param name="state">Состояние</param>
        public void ChangeState(TArg arg, UState state) => _stateChanged?.Invoke(arg, state);
    }
}
