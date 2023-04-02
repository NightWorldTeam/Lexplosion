using System;
using System.Windows.Media;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class LowerButton : VMBase, IComparable
    {
        private string _text;
        private Geometry _icon;
        private LowerButtonFunc _func;
        private int _index = 0;

        public LowerButton(string text, Geometry icon, LowerButtonFunc func, int index = 0)
        {
            Text = text;
            Icon = icon;
            Func = func;
            _index = index;
        }

        public string Text
        {
            get => _text; set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public Geometry Icon
        {
            get => _icon; set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public LowerButtonFunc Func
        {
            get => _func; set
            {
                _func = value;
                OnPropertyChanged();
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is LowerButton lowerButton) return lowerButton._index.CompareTo(this._index);
            else throw new ArgumentException("Некорректное значение параметра");
        }
    }
}
