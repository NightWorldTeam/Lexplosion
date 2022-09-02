using System.Windows.Media;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class LowerButton : VMBase
    {
        private string _text;
        private Geometry _icon;
        private LowerButtonFunc _func;

        public LowerButton(string text, Geometry icon, LowerButtonFunc func)
        {
            Text = text;
            Icon = icon;
            Func = func;
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
    }
}
