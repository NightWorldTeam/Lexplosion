namespace Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects
{
    public readonly struct SortByParamObject : ITranslatableObject<string>
    {
        public string TranslateKey { get; }
        public string Value { get; }

        public SortByParamObject(string translateKey, string value)
        {
            TranslateKey = translateKey;
            Value = value;
        }
    }
}
