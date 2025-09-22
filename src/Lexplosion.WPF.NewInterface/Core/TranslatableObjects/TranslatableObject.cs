namespace Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects
{
    /// <summary>
    /// Базовая реализация ITranslatableObject
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct TranslatableObject<T> : ITranslatableObject<T>
    {
        /// <summary>
        /// Ключ словаря с переводом
        /// </summary>
        public string TranslateKey { get; }
        /// <summary>
        /// Значение
        /// </summary>
        public T Value { get; }


        public TranslatableObject(string translateKey, T value)
        {
            TranslateKey = translateKey;
            Value = value;
        }
    }
}
