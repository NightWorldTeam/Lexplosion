using System;

namespace Lexplosion.Gui.TrayMenu
{
    public abstract class TrayCompontent : IComparable
    {
        protected int _id;
        
        /// <summary>
        /// Если true, то компонент Трея не будет виден и занимать место.
        /// </summary>
        public bool IsEnable { get; set; }

        public int CompareTo(object obj)
        {
            return ((int)obj).CompareTo(_id);
        }
    }
}
