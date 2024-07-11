namespace Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects
{
    public interface ITranslatableObject<T>
    {
        public string TranslateKey { get; }
        public T Value { get; }
    }
}
