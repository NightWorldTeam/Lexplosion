using System;

namespace Lexplosion.WPF.NewInterface.Stores
{
    public interface INavigationStore<T>
    {
        event Action CurrentViewModelChanged;

        T Content { get; set; }

        bool IsOpen { get; }

        void Open(T content);
        void Close();

    }
}
