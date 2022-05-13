using System.Windows.Media;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public class MultibuttonModel : VMBase
    {
        private UpperButtonFunc _upperFunc;
        private LowerButtonFunc _lowerFunc;
        private Geometry _upperIcon;
        private Geometry _lowerIcon;
        private Tip _upperTip;
        private Tip _lowerTip;

        public UpperButtonFunc UpperFunc 
        {
            get => _upperFunc; set 
            {
                _upperFunc = value;
                OnPropertyChanged(nameof(UpperFunc));
            }
        }

        public LowerButtonFunc LowerFunc
        {
            get => _lowerFunc; set
            {
                _lowerFunc = value;
                OnPropertyChanged(nameof(LowerFunc));
            }
        }

        public Geometry UpperIcon
        {
            get => _upperIcon; set
            {
                _upperIcon = value;
                OnPropertyChanged("UpperIcon");
            }
        }

        public Geometry LowerIcon
        {
            get => _lowerIcon; set
            {
                _lowerIcon = value;
                OnPropertyChanged("LowerIcon");
            }
        }

        public Tip UpperTip
        {
            get => _upperTip; set
            {
                _upperTip = value;
                OnPropertyChanged("UpperTip");
            }
        }
        public Tip LowerTip
        {
            get => _lowerTip; set
            {
                _lowerTip = value;
                OnPropertyChanged("LowerTip");
            }
        }

        public void ChangeFuncPlay()
            => ChangeFunc(
                    UpperButtonFunc.Play, LowerButtonFunc.OpenFolder,
                    MultiButtonProperties.GeometryPlayIcon, MultiButtonProperties.GeometryOpenFolder,
                    "Играть", -60, "Открыть папку с игрой", -150
            );

        public void ChangeFuncDownload(bool isAddedToLibrary)
        {
            var lowerBtnFunc = LowerButtonFunc.AddToLibrary;
            if (isAddedToLibrary)
                lowerBtnFunc = LowerButtonFunc.DeleteFromLibrary;

            ChangeFunc(
                UpperButtonFunc.Download, lowerBtnFunc,
                MultiButtonProperties.GeometryDownloadIcon, MultiButtonProperties.GeometryLibraryDelete,
                "Скачать сборку", -110, "Удалить из библиотеке", -150
                );
        }

        public void ChangeFuncProgressBar() 
            => ChangeFunc(
                    UpperButtonFunc.ProgressBar, LowerButtonFunc.CancelDownload,
                    null, MultiButtonProperties.GeometryCancelIcon,
                    "Скачивание завершено на", -160, "Отменить скачивание", -140
                );

        public void ChangeFuncClose() 
            => ChangeFunc(
                    UpperButtonFunc.Close, LowerButtonFunc.OpenFolder,
                    MultiButtonProperties.GeometryCancelIcon, MultiButtonProperties.GeometryOpenFolder,
                    "Закрыть игру", -100, "Открыть папку с игрой", -150
                );

        public void ChangeFunc(
            UpperButtonFunc uFunc, LowerButtonFunc lFunc, Geometry uIcon,
            Geometry lIcon, string uTipText, int uOffset, string lTipText, int lOffset
            )
        {
            UpperFunc = uFunc;
            LowerFunc = lFunc;
            UpperIcon = uIcon;
            LowerIcon = lIcon;
            UpperTip = new Tip
            {
                Text = uTipText,
                Offset = uOffset
            };
            LowerTip = new Tip
            {
                Text = lTipText,
                Offset = lOffset
            };
        }
    }
    public struct Tip
    {
        public string Text { get; set; }
        public int Offset { get; set; }
    }

    public enum UpperButtonFunc
    {
        Download,
        Play,
        ProgressBar,
        Update,
        Close
    }

    public enum LowerButtonFunc
    {
        AddToLibrary,
        DeleteFromLibrary,
        OpenFolder,
        CancelDownload
    }

    public class MultiButtonProperties
    {
        /**
         * Pathes (Images on button)
         */
        public static readonly Geometry GeometryDownloadIcon = Geometry.Parse("M 50.625 0 h 18.75 A 5.612 5.612 0 0 1 75 5.625 V 45 H 95.555 a 4.679 4.679 0 0 1 3.3 7.992 L 63.211 88.664 a 4.541 4.541 0 0 1 -6.4 0 l -35.7 -35.672 A 4.679 4.679 0 0 1 24.422 45 H 45 V 5.625 A 5.612 5.612 0 0 1 50.625 0 Z M 120 88.125 v 26.25 A 5.612 5.612 0 0 1 114.375 120 H 5.625 A 5.612 5.612 0 0 1 0 114.375 V 88.125 A 5.612 5.612 0 0 1 5.625 82.5 H 40.008 L 51.492 93.984 a 12.01 12.01 0 0 0 17.016 0 L 79.992 82.5 h 34.383 A 5.612 5.612 0 0 1 120 88.125 Z M 90.938 108.75 a 4.688 4.688 0 1 0 -4.687 4.688 A 4.7 4.7 0 0 0 90.938 108.75 Z m 15 0 a 4.688 4.688 0 1 0 -4.687 4.688 A 4.7 4.7 0 0 0 105.938 108.75 Z");
        public static readonly Geometry GeometryPauseIcon = Geometry.Parse("M0.666992 0.833374H3.16699V9.16671H0.666992V0.833374ZM4.83366 0.833374H7.33366V9.16671H4.83366V0.833374Z");
        public static readonly Geometry GeometryCancelIcon = Geometry.Parse("M9.00033 0.666626C4.39199 0.666626 0.666992 4.39163 0.666992 8.99996C0.666992 13.6083 4.39199 17.3333 9.00033 17.3333C13.6087 17.3333 17.3337 13.6083 17.3337 8.99996C17.3337 4.39163 13.6087 0.666626 9.00033 0.666626ZM12.5837 12.5833C12.5066 12.6605 12.415 12.7218 12.3142 12.7637C12.2134 12.8055 12.1053 12.827 11.9962 12.827C11.887 12.827 11.7789 12.8055 11.6781 12.7637C11.5773 12.7218 11.4858 12.6605 11.4087 12.5833L9.00033 10.175L6.59199 12.5833C6.43618 12.7391 6.22485 12.8266 6.00449 12.8266C5.78414 12.8266 5.57281 12.7391 5.41699 12.5833C5.26118 12.4275 5.17364 12.2161 5.17364 11.9958C5.17364 11.8867 5.19513 11.7786 5.23689 11.6778C5.27864 11.577 5.33984 11.4854 5.41699 11.4083L7.82533 8.99996L5.41699 6.59163C5.26118 6.43581 5.17364 6.22448 5.17364 6.00413C5.17364 5.78377 5.26118 5.57244 5.41699 5.41663C5.57281 5.26081 5.78414 5.17328 6.00449 5.17328C6.22485 5.17328 6.43618 5.26081 6.59199 5.41663L9.00033 7.82496L11.4087 5.41663C11.4858 5.33947 11.5774 5.27827 11.6782 5.23652C11.779 5.19477 11.887 5.17328 11.9962 5.17328C12.1053 5.17328 12.2133 5.19477 12.3141 5.23652C12.4149 5.27827 12.5065 5.33947 12.5837 5.41663C12.6608 5.49378 12.722 5.58537 12.7638 5.68617C12.8055 5.78698 12.827 5.89502 12.827 6.00413C12.827 6.11323 12.8055 6.22127 12.7638 6.32208C12.722 6.42288 12.6608 6.51447 12.5837 6.59163L10.1753 8.99996L12.5837 11.4083C12.9003 11.725 12.9003 12.2583 12.5837 12.5833Z");
        public static readonly Geometry GeometryPlayIcon = Geometry.Parse("M0 0V28L22 14L0 0Z");
        public static readonly Geometry GeometryLibraryAdd = Geometry.Parse("M0 3.5C0 2.57174 0.368749 1.6815 1.02513 1.02513C1.6815 0.368749 2.57174 0 3.5 0H5.83333C6.76159 0 7.65183 0.368749 8.30821 1.02513C8.96458 1.6815 9.33333 2.57174 9.33333 3.5V5.83333C9.33333 6.76159 8.96458 7.65183 8.30821 8.30821C7.65183 8.96458 6.76159 9.33333 5.83333 9.33333H3.5C2.57174 9.33333 1.6815 8.96458 1.02513 8.30821C0.368749 7.65183 0 6.76159 0 5.83333V3.5ZM0 15.1667C0 14.2384 0.368749 13.3482 1.02513 12.6918C1.6815 12.0354 2.57174 11.6667 3.5 11.6667H5.83333C6.76159 11.6667 7.65183 12.0354 8.30821 12.6918C8.96458 13.3482 9.33333 14.2384 9.33333 15.1667V17.5C9.33333 18.4283 8.96458 19.3185 8.30821 19.9749C7.65183 20.6313 6.76159 21 5.83333 21H3.5C2.57174 21 1.6815 20.6313 1.02513 19.9749C0.368749 19.3185 0 18.4283 0 17.5V15.1667ZM11.6667 3.5C11.6667 2.57174 12.0354 1.6815 12.6918 1.02513C13.3482 0.368749 14.2384 0 15.1667 0H17.5C18.4283 0 19.3185 0.368749 19.9749 1.02513C20.6313 1.6815 21 2.57174 21 3.5V5.83333C21 6.76159 20.6313 7.65183 19.9749 8.30821C19.3185 8.96458 18.4283 9.33333 17.5 9.33333H15.1667C14.2384 9.33333 13.3482 8.96458 12.6918 8.30821C12.0354 7.65183 11.6667 6.76159 11.6667 5.83333V3.5ZM17.5 12.8333C17.5 12.5239 17.3771 12.2272 17.1583 12.0084C16.9395 11.7896 16.6428 11.6667 16.3333 11.6667C16.0239 11.6667 15.7272 11.7896 15.5084 12.0084C15.2896 12.2272 15.1667 12.5239 15.1667 12.8333V15.1667H12.8333C12.5239 15.1667 12.2272 15.2896 12.0084 15.5084C11.7896 15.7272 11.6667 16.0239 11.6667 16.3333C11.6667 16.6428 11.7896 16.9395 12.0084 17.1583C12.2272 17.3771 12.5239 17.5 12.8333 17.5H15.1667V19.8333C15.1667 20.1428 15.2896 20.4395 15.5084 20.6583C15.7272 20.8771 16.0239 21 16.3333 21C16.6428 21 16.9395 20.8771 17.1583 20.6583C17.3771 20.4395 17.5 20.1428 17.5 19.8333V17.5H19.8333C20.1428 17.5 20.4395 17.3771 20.6583 17.1583C20.8771 16.9395 21 16.6428 21 16.3333C21 16.0239 20.8771 15.7272 20.6583 15.5084C20.4395 15.2896 20.1428 15.1667 19.8333 15.1667H17.5V12.8333Z");
        public static readonly Geometry GeometryLibraryDelete = Geometry.Parse("M19.1667 8.66667H12.1667V21.5H2.83333C2.21449 21.5 1.621 21.2542 1.18342 20.8166C0.745833 20.379 0.5 19.7855 0.5 19.1667V2.83333C0.5 2.21449 0.745833 1.621 1.18342 1.18342C1.621 0.745833 2.21449 0.5 2.83333 0.5H19.1667C19.7855 0.5 20.379 0.745833 20.8166 1.18342C21.2542 1.621 21.5 2.21449 21.5 2.83333V12.1667H19.1667V8.66667ZM9.83333 8.66667H2.83333V13.3333H9.83333V8.66667ZM9.83333 19.1667V15.6667H2.83333V19.1667H9.83333ZM12.1667 2.83333V6.33333H19.1667V2.83333H12.1667ZM9.83333 2.83333H2.83333V6.33333H9.83333V2.83333Z");
        public static readonly Geometry GeometryOpenFolder = Geometry.Parse("M0.804492 13.2924C0.959492 13.5258 1.22033 13.6666 1.50033 13.6666H14.0003C14.3337 13.6666 14.6353 13.4683 14.7662 13.1616L17.2662 7.32825C17.3209 7.20153 17.3433 7.06317 17.3313 6.92564C17.3192 6.78812 17.2731 6.65575 17.1971 6.54049C17.1211 6.42523 17.0177 6.3307 16.896 6.26542C16.7744 6.20015 16.6384 6.16618 16.5003 6.16659H15.667V3.66659C15.667 2.74742 14.9195 1.99992 14.0003 1.99992H8.45449L6.32449 0.333252H2.33366C1.41449 0.333252 0.666992 1.08075 0.666992 1.99992V12.8333H0.672825C0.670968 12.9959 0.716738 13.1555 0.804492 13.2924V13.2924ZM14.0003 3.66659V6.16659H4.00033C3.66699 6.16659 3.36533 6.36492 3.23449 6.67159L2.33366 8.77408V3.66659H14.0003Z");
    }
}
