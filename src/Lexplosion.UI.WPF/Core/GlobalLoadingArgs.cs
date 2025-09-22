namespace Lexplosion.UI.WPF.Core
{
    public readonly struct GlobalLoadingArgs
    {
        public bool State { get; }
        public string ProcessDescription { get; }

        public GlobalLoadingArgs(bool state, string processDescription)
        {
            State = state;
            ProcessDescription = processDescription;
        }
    }
}
