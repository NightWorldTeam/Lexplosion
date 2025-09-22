namespace Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects
{
    public readonly struct SortByParamObject : ITranslatableObject<int>
    {
        public string TranslateKey { get; }
        public int Value { get; }

        public SortByParamObject(string translateKey, int value)
        {
            TranslateKey = translateKey;
            Value = value;
        }
    }
}
