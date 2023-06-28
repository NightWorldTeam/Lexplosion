using System;

namespace Lexplosion.WPF.NewInterface.Stores
{
    public sealed class NavigationStore : INavigationStore<VMBase>
    {
        public event Action CurrentViewModelChanged;


        #region Properties


        private VMBase _content;
        public VMBase Content
        {
            get => _content; set
            {
                _content = value;
                OnCurrentViewModelChanged();
            }
        }

        public bool IsOpen => _content != null;


        #endregion Properties


        #region Public Methods


        public void Close()
        {
            Content = null;
        }

        public void Open(VMBase content)
        {
            Content = content;
        }


        #endregion Public Methods


        #region Private Methods


        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }


        #endregion Private Methods
    }
}
