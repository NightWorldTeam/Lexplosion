using Lexplosion.WPF.NewInterface.Commands;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Controls
{
#if DEBUG
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
#endif
#pragma warning disable U2U1004 // Public value types should implement equality
    public struct LowerMenuButton : IEquatable<LowerMenuButton>
#pragma warning restore U2U1004 // Public value types should implement equality
    {
        public int Id { get; }
        public string IconKey { get; }
        public string TextKey { get; }

        private Action<object> _action;
        private RelayCommand _executeAction;
        public ICommand ExecuteAction
        {
            get => _executeAction ?? (_executeAction = new RelayCommand(_action));
        }



        public LowerMenuButton(int id, string iconKey, string titleKey, Action action)
        {
            Id = id;
            IconKey = iconKey;
            TextKey = titleKey;
            _action = obj => { action(); };
        }


        #region Public Methods


        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(LowerMenuButton other)
        {
            return other.Id == this.Id && other.TextKey == this.TextKey && other.IconKey == this.IconKey;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }


        #endregion Public Methods


#if DEBUG


        private string GetDebuggerDisplay()
        {
            return $"LowerMenuButton {Id} - {TextKey}";
        }


#endif
    }
}
