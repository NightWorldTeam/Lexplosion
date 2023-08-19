using System;
using System.Diagnostics;

namespace Lexplosion.WPF.NewInterface.Controls
{
#if DEBUG
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
#endif
#pragma warning disable U2U1004 // Public value types should implement equality
    public struct LowerMenuButton : IEquatable<LowerMenuButton>
#pragma warning restore U2U1004 // Public value types should implement equality
    {
        public int Id;
        public string IconKey { get; }
        public string TextKey { get; }
        public Action Action;


        public LowerMenuButton(int id, string iconKey, string titleKey, Action action)
        {
            Id = id;
            IconKey = iconKey;
            TextKey = titleKey;
            Action = action;
        }


#region Public Methods


        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(LowerMenuButton other)
        {
            return other.Id == this.Id && other.TextKey == this.TextKey && other.IconKey == this.IconKey && other.Action == this.Action; 
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
