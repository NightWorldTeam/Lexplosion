using Lexplosion.Global;
using Lexplosion.Gui.InstanceCreator;
using Lexplosion.Gui.Pages;
using Lexplosion.Gui.Pages.Instance;
using Lexplosion.Gui.Pages.MW;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Interaction logic for LeftPanel.xaml
    /// </summary>
    public partial class LeftPanel : UserControl
    {
        enum Functions
        {
            Initialize,
            // Главное меню
            Catalog,
            Library,
            Multiplayer,
            LauncherSettings,
            // Профиль сборки
            Instance,
            InstanceSettings,
            // Установки
            Vanilla,
            AddInstance,
            // Другое
            Back
        }

        public enum PageType
        {
            InstanceContainer,
            InstanceLibrary,
            MultiplayerContainer,
            LauncherSettings,
            OpenedInstanceShowCase,
            OpenedInstance,
            Installers,
        }

        private PageType _activePageType;
        private MainWindow _mainWindow;
        private InstanceProperties _instanceProperties;

        private List<ToggleButton> _toggleButtons = new List<ToggleButton>();
        private Dictionary<ToggleButton, Functions> _buttons = new Dictionary<ToggleButton, Functions>();

        public LeftPanel(Page obj, PageType page, MainWindow mw)
        {
            InitializeComponent();

            _activePageType = page;
            _mainWindow = mw;

            _toggleButtons.Add(MenuButton0);
            _toggleButtons.Add(MenuButton1);
            _toggleButtons.Add(MenuButton2);
            _toggleButtons.Add(MenuButton3);
            
            MenuButton0.IsChecked = true;
            ReselectionButton(MenuButton0);
            InitializeContent("Каталог", "Библиотека", "Сетевая игра", "Настройки");
            InstanceForm.InstanceOpened += InstancePageSelected;
            SetupUserLogin();
        }


        private void ReselectionButton(ToggleButton selectedButton)
        {
            foreach (ToggleButton toggleButton in _toggleButtons)
            {
                if (toggleButton != selectedButton)
                {
                    toggleButton.IsEnabled = true;
                    toggleButton.IsChecked = false;
                }
                else
                {
                    selectedButton.IsChecked = true;
                    selectedButton.IsEnabled = false;
                }
            }
        }
    }
}