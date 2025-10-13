using System;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Args
{
    public class MultiplayerLayoutArgs
    {
        public readonly Action OpenAccountFactory;
        public readonly SelectInstanceForServerArgs SelectInstanceForServerArgs;

        public MultiplayerLayoutArgs(Action openAccountFactory, SelectInstanceForServerArgs selectInstanceForServerArgs)
        {
            OpenAccountFactory = openAccountFactory;
            SelectInstanceForServerArgs = selectInstanceForServerArgs;
        }
    }
}
