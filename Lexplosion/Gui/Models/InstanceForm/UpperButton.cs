using Lexplosion.Tools;
using System.Windows.Media;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public sealed class UpperButton : VMBase
    {
        private Geometry _icon;
        private UpperButtonFunc _func;
        private Tip _tip;

        public UpperButton(Geometry icon, UpperButtonFunc func, Tip tip)
        {
            Icon = icon;
            Func = func;
            Tip = tip;
        }

        #region props
        public Geometry Icon
        {
            get => _icon; set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
        public UpperButtonFunc Func
        {
            get => _func; set
            {
                _func = value;
                OnPropertyChanged();
            }
        }
        public Tip Tip
        {
            get => _tip; set
            {
                _tip = value;
                OnPropertyChanged();
            }
        }

        #endregion props

        #region methods
        public void ChangeFuncPlay()
            => ChangeFunc(
                    UpperButtonFunc.Play,
                    ResourceGetter.GetIcon("Play"),
                    "Играть", -60
            );

        public void ChangeFuncDownload()
        {
            ChangeFunc(
                UpperButtonFunc.Download,
                ResourceGetter.GetIcon("Download"),
                "Скачать сборку", -110
                );
        }

        public void ChangeFuncProgressBar()
            => ChangeFunc(
                    UpperButtonFunc.ProgressBar,
                    null,
                    "Скачивание завершено на", -160
                );

        public void ChangeFuncClose()
            => ChangeFunc(
                    UpperButtonFunc.Close,
                    MultiButtonProperties.GeometryCancelIcon,
                    "Закрыть игру", -100
                );

        public void ChangeFunc(
            UpperButtonFunc func, Geometry icon, string tipText, int offset
            )
        {
            Func = func;
            Icon = icon;
            Tip = new Tip
            {
                Text = tipText,
                Offset = offset
            };
        }
    }
    #endregion methods
}