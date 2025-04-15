namespace Lexplosion.WPF.NewInterface.Core.Objects.TranslatableObjects
{
    public readonly struct InstanceSourceObject : ITranslatableObject<InstanceSource>
    {
        public string TranslateKey { get; }
        public InstanceSource Value { get; }

        public InstanceSourceObject(string translateKey, InstanceSource instanceSource)
        {
            TranslateKey = translateKey;
            Value = instanceSource;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
