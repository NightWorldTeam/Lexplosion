using System;

namespace Lexplosion.Gui.TrayMenu
{
    public abstract class TrayCompontent : VMBase, IComparable
    {
        protected int _id;

        /// <summary>
        /// Если true, то компонент Трея не будет виден и занимать место.
        /// </summary>
        private bool _isEnable;
        public bool IsEnable 
        { 
            get => _isEnable; set 
            {
                _isEnable = value;
                OnPropertyChanged();
            }
        }

        public int CompareTo(object obj)
        {
            return ((int)obj).CompareTo(_id);
        }
    }
}
