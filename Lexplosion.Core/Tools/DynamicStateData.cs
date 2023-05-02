using System;

namespace Lexplosion.Tools
{
    /// <summary>
    /// Нужен для передачи информации об изменении какого-то состояния.
    /// </summary>
    /// <typeparam name="T">Тип аргумента</typeparam>
    /// <typeparam name="U">Тип состояния</typeparam>
    public class DynamicStateData<T, U>
    {
        /// <summary>
        /// Возвращает хэндлер, с помощью которого можно вызывать событие StateChanged.
        /// </summary>
        public DynamicStateHandler<T, U> GetHandler
        {
            get
            {
                return new DynamicStateHandler<T, U>(StateChanged);
            }
        }

        /// <summary>
        /// Вызывается когда происходит измнение состояния.
        /// </summary>
        public event Action<T, U> StateChanged;
    }

    /// <summary>
    /// Генерируется классом DynamicStateData и нужен для обновления состояния
    /// </summary>
    /// <typeparam name="T">Тип аргумента</typeparam>
    /// <typeparam name="U">Тип состояния</typeparam>
    public struct DynamicStateHandler<T, U>
    {
        private Action<T, U> _stateChanged;

        public DynamicStateHandler(Action<T, U> stateChangeHandler)
        {
            _stateChanged = stateChangeHandler;
        }

        /// <summary>
        /// Сообщает об изменении состояния.
        /// </summary>
        /// <param name="arg">Аргумент</param>
        /// <param name="state">Состояние</param>
        public void ChangeState(T arg, U state) => _stateChanged?.Invoke(arg, state);
    }
}
