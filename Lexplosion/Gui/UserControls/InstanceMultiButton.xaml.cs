using Lexplosion.Global;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for MultifunctionButtonInstance.xaml
    /// </summary>
    public partial class InstanceMultiButton : UserControl
    {
        public enum ButtonTypes
        {
            Basic,
            Library,
            Folder
        }

        enum ButtonFunctions
        {
            Play,
            Download,
            LibraryAdd,
            LibraryDelete,
            OpenFolder
        }

        class LocalInstanceData
        {
            /* int */
            public int CurseforgeInstanceId;
            /* bool */
            public bool IsInstalled;
            public bool IsAddedToLibrary;
        }
        /**
         * Pathes (Images on button)
         */
        private readonly Geometry _geometryDownloadIcon = Geometry.Parse("M 50.625 0 h 18.75 A 5.612 5.612 0 0 1 75 5.625 V 45 H 95.555 a 4.679 4.679 0 0 1 3.3 7.992 L 63.211 88.664 a 4.541 4.541 0 0 1 -6.4 0 l -35.7 -35.672 A 4.679 4.679 0 0 1 24.422 45 H 45 V 5.625 A 5.612 5.612 0 0 1 50.625 0 Z M 120 88.125 v 26.25 A 5.612 5.612 0 0 1 114.375 120 H 5.625 A 5.612 5.612 0 0 1 0 114.375 V 88.125 A 5.612 5.612 0 0 1 5.625 82.5 H 40.008 L 51.492 93.984 a 12.01 12.01 0 0 0 17.016 0 L 79.992 82.5 h 34.383 A 5.612 5.612 0 0 1 120 88.125 Z M 90.938 108.75 a 4.688 4.688 0 1 0 -4.687 4.688 A 4.7 4.7 0 0 0 90.938 108.75 Z m 15 0 a 4.688 4.688 0 1 0 -4.687 4.688 A 4.7 4.7 0 0 0 105.938 108.75 Z");
        private readonly Geometry _geometryPlayIcon = Geometry.Parse("M0 0V28L22 14L0 0Z");
        private readonly Geometry _geometryLibraryAdd = Geometry.Parse("M0 3.5C0 2.57174 0.368749 1.6815 1.02513 1.02513C1.6815 0.368749 2.57174 0 3.5 0H5.83333C6.76159 0 7.65183 0.368749 8.30821 1.02513C8.96458 1.6815 9.33333 2.57174 9.33333 3.5V5.83333C9.33333 6.76159 8.96458 7.65183 8.30821 8.30821C7.65183 8.96458 6.76159 9.33333 5.83333 9.33333H3.5C2.57174 9.33333 1.6815 8.96458 1.02513 8.30821C0.368749 7.65183 0 6.76159 0 5.83333V3.5ZM0 15.1667C0 14.2384 0.368749 13.3482 1.02513 12.6918C1.6815 12.0354 2.57174 11.6667 3.5 11.6667H5.83333C6.76159 11.6667 7.65183 12.0354 8.30821 12.6918C8.96458 13.3482 9.33333 14.2384 9.33333 15.1667V17.5C9.33333 18.4283 8.96458 19.3185 8.30821 19.9749C7.65183 20.6313 6.76159 21 5.83333 21H3.5C2.57174 21 1.6815 20.6313 1.02513 19.9749C0.368749 19.3185 0 18.4283 0 17.5V15.1667ZM11.6667 3.5C11.6667 2.57174 12.0354 1.6815 12.6918 1.02513C13.3482 0.368749 14.2384 0 15.1667 0H17.5C18.4283 0 19.3185 0.368749 19.9749 1.02513C20.6313 1.6815 21 2.57174 21 3.5V5.83333C21 6.76159 20.6313 7.65183 19.9749 8.30821C19.3185 8.96458 18.4283 9.33333 17.5 9.33333H15.1667C14.2384 9.33333 13.3482 8.96458 12.6918 8.30821C12.0354 7.65183 11.6667 6.76159 11.6667 5.83333V3.5ZM17.5 12.8333C17.5 12.5239 17.3771 12.2272 17.1583 12.0084C16.9395 11.7896 16.6428 11.6667 16.3333 11.6667C16.0239 11.6667 15.7272 11.7896 15.5084 12.0084C15.2896 12.2272 15.1667 12.5239 15.1667 12.8333V15.1667H12.8333C12.5239 15.1667 12.2272 15.2896 12.0084 15.5084C11.7896 15.7272 11.6667 16.0239 11.6667 16.3333C11.6667 16.6428 11.7896 16.9395 12.0084 17.1583C12.2272 17.3771 12.5239 17.5 12.8333 17.5H15.1667V19.8333C15.1667 20.1428 15.2896 20.4395 15.5084 20.6583C15.7272 20.8771 16.0239 21 16.3333 21C16.6428 21 16.9395 20.8771 17.1583 20.6583C17.3771 20.4395 17.5 20.1428 17.5 19.8333V17.5H19.8333C20.1428 17.5 20.4395 17.3771 20.6583 17.1583C20.8771 16.9395 21 16.6428 21 16.3333C21 16.0239 20.8771 15.7272 20.6583 15.5084C20.4395 15.2896 20.1428 15.1667 19.8333 15.1667H17.5V12.8333Z");
        private readonly Geometry _geometryLibraryDelete = Geometry.Parse("M19.1667 8.66667H12.1667V21.5H2.83333C2.21449 21.5 1.621 21.2542 1.18342 20.8166C0.745833 20.379 0.5 19.7855 0.5 19.1667V2.83333C0.5 2.21449 0.745833 1.621 1.18342 1.18342C1.621 0.745833 2.21449 0.5 2.83333 0.5H19.1667C19.7855 0.5 20.379 0.745833 20.8166 1.18342C21.2542 1.621 21.5 2.21449 21.5 2.83333V12.1667H19.1667V8.66667ZM9.83333 8.66667H2.83333V13.3333H9.83333V8.66667ZM9.83333 19.1667V15.6667H2.83333V19.1667H9.83333ZM12.1667 2.83333V6.33333H19.1667V2.83333H12.1667ZM9.83333 2.83333H2.83333V6.33333H9.83333V2.83333Z");
        private readonly Geometry _geometryOpenFolder = Geometry.Parse("M0.804492 13.2924C0.959492 13.5258 1.22033 13.6666 1.50033 13.6666H14.0003C14.3337 13.6666 14.6353 13.4683 14.7662 13.1616L17.2662 7.32825C17.3209 7.20153 17.3433 7.06317 17.3313 6.92564C17.3192 6.78812 17.2731 6.65575 17.1971 6.54049C17.1211 6.42523 17.0177 6.3307 16.896 6.26542C16.7744 6.20015 16.6384 6.16618 16.5003 6.16659H15.667V3.66659C15.667 2.74742 14.9195 1.99992 14.0003 1.99992H8.45449L6.32449 0.333252H2.33366C1.41449 0.333252 0.666992 1.08075 0.666992 1.99992V12.8333H0.672825C0.670968 12.9959 0.716738 13.1555 0.804492 13.2924V13.2924ZM14.0003 3.66659V6.16659H4.00033C3.66699 6.16659 3.36533 6.36492 3.23449 6.67159L2.33366 8.77408V3.66659H14.0003Z");

        /**
         * Colors when user Enter and Leave to button
         */
        private readonly Color _mouseEnterColor = System.Windows.Media.Color.FromArgb(255, 22, 127, 252);
        private readonly Color _mouseLeaveColor = System.Windows.Media.Color.FromArgb(255, 21, 23, 25);

        /**
         * State button when modpack installed or not installed
         */
        private ButtonTypes _selectedType;
        private ButtonFunctions _selectedFunction;

        /**
         * Instance object method
         */
        public InstanceMultiButton(ButtonTypes selectedType, bool isInstalledInstance, bool isAddedToLibrary)
        {
            InitializeComponent();
            background.Color = _mouseLeaveColor;
            LocalInstanceData instanceData = new LocalInstanceData()
            {
                IsInstalled = isInstalledInstance,
                IsAddedToLibrary = isAddedToLibrary,
            };
            _selectedType = selectedType;
            SetButtonFunction();
        }

        /**
         * Called when user enter cursor to button
         */
        private void DownloadLaunchButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = _mouseLeaveColor,
                To = _mouseEnterColor,
                Duration = TimeSpan.FromSeconds(0.2),
            };
            background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        /**
         * Called when user leave cursor from button
         */
        private void DownloadLaunchButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = _mouseEnterColor,
                To = _mouseLeaveColor,
                Duration = TimeSpan.FromSeconds(0.2),
            };
            background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        /**
         * Called when the mouse is clicked left button.
         */
        private void SetButtonFunction() 
        {
            LocalInstanceData instanceData = new LocalInstanceData();
            switch (_selectedType) 
            {
                case ButtonTypes.Basic:
                    if (instanceData.IsInstalled)
                    {
                        InstanceLaunchPath.Data = _geometryPlayIcon;
                        Tooltips.HorizontalOffset = -60;
                        ToolTipLable.Content = "Играть";
                    }
                    else
                    {
                        InstanceLaunchPath.Data = _geometryDownloadIcon;
                        Tooltips.HorizontalOffset = -160;
                        ToolTipLable.Content = "Скачать сборку";
                    }
                    break;
                case ButtonTypes.Library:
                    if (instanceData.IsAddedToLibrary) 
                    {
                        InstanceLaunchPath.Data = _geometryLibraryAdd;
                        Tooltips.HorizontalOffset = -160;
                        ToolTipLable.Content = "Добавить в библиотеку";
                    }
                    else
                    {
                        InstanceLaunchPath.Data = _geometryLibraryAdd;
                        Tooltips.HorizontalOffset = -160;
                        ToolTipLable.Content = "Удалить из библиотеки";
                    }
                    break;
                case ButtonTypes.Folder:
                    if (instanceData.IsInstalled) 
                    {
                        InstanceLaunchPath.Data = _geometryOpenFolder;
                        Tooltips.HorizontalOffset = -160;
                        ToolTipLable.Content = "Открыть папку с игрой";
                    }
                    else
                    {
                        Tooltips.Width = 0;
                    }
                    break;
            }
        }


        public void IsInstalledInstance() 
        {
            // start game here
        }
    }
}
