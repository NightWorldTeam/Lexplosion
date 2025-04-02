using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Controls.OldInstanceForm
{
    /// <summary>
    /// Interaction logic for OldInstanceForm.xaml
    /// </summary>
    public partial class OldInstanceForm : UserControl
    {
        private InstanceModelBase _model;


        #region Dependency Properties


        public static readonly DependencyProperty LogoButtonCommandProperty
            = DependencyProperty.Register(nameof(LogoButtonCommand), typeof(ICommand), typeof(OldInstanceForm),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty LogoButtonCommandParameterProperty
            = DependencyProperty.Register(nameof(LogoButtonCommandParameter), typeof(object), typeof(OldInstanceForm),
                new FrameworkPropertyMetadata(defaultValue: (object)null));

        public static readonly DependencyProperty OpenAddonsPageCommandProperty
            = DependencyProperty.Register(nameof(OpenAddonsPageCommand), typeof(ICommand), typeof(OldInstanceForm),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty CanBeDeletedProperty
            = DependencyProperty.Register(nameof(CanBeDeleted), typeof(bool), typeof(OldInstanceForm),
            new FrameworkPropertyMetadata(false));

        public bool CanBeDeleted
        {
            get => (bool)GetValue(CanBeDeletedProperty);
            set => SetValue(CanBeDeletedProperty, value);
        }

        public ICommand LogoButtonCommand
        {
            get => (ICommand)GetValue(LogoButtonCommandProperty);
            set => SetValue(LogoButtonCommandProperty, value);
        }

        public ICommand OpenAddonsPageCommand
        {
            get => (ICommand)GetValue(OpenAddonsPageCommandProperty);
            set => SetValue(OpenAddonsPageCommandProperty, value);
        }

        public object LogoButtonCommandParameter
        {
            get => (object)GetValue(LogoButtonCommandParameterProperty);
            set => SetValue(LogoButtonCommandParameterProperty, value);
        }


        #endregion Dependency Properties


        public OldInstanceForm()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _model = (InstanceModelBase)DataContext;
            _model.DeletedEvent += OnInstanceDeleted;
            InstanceModel_ModloaderChanged();
            SetVisitButtonIconAndText();
        }

        private void SetVisitButtonIconAndText() 
        {
            VisitWebsiteButton.SetResourceReference(AdvancedButton.IconDataProperty, $"PD{_model.Source}");
            VisitWebsiteButton.SetResourceReference(AdvancedButton.TextProperty, $"Visit{_model.Source}");
        }

        private void InstanceModel_ModloaderChanged()
        {
            var basePath = "pack://application:,,,/Assets/images/icons/";

            if (_model.BaseData.Modloader != ClientType.Vanilla)
            {
                ModloaderIcon.Background = new ImageBrush(new BitmapImage(new Uri($"{basePath}{_model.BaseData.Modloader.ToString().ToLower()}.png", UriKind.Absolute)));
                ModloaderIcon.ToolTip = _model.BaseData.Modloader.ToString();
                ModloaderIcon.Visibility = Visibility.Visible;
                return;
            }

            ModloaderIcon.Visibility = Visibility.Collapsed;
        }

        private void PART_MainActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model.IsInstalled && !_model.IsLaunching && !_model.IsLaunched)
            {
                _model.Run();
                return;
            }

            if (_model.IsDownloading)
            {
                // TODO: Открыть меню со списком файлов
                return;
            }

            if (_model.IsLaunched || _model.IsLaunching)
            {
                _model.Close();
                return;
            }

            if (_model.IsShareDownloading) 
            {
                _model.CancelShareInstanceDownloading();
            }

            _model.Download();
        }

        #region Lower Buttons Click


        /// <summary>
        /// Отмена скачивания
        /// </summary>
        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.CancelDownload();
        }

        /// <summary>
        /// Открытие папки
        /// </summary>
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.OpenFolder();
        }

        /// <summary>
        /// Открытие меню экспорта
        /// </summary>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.Export();
        }

        /// <summary>
        /// Открыть менеджер аддонов
        /// </summary>
        private void OpenAddonManagerButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            OpenAddonsPageCommand.Execute(_model);
        }

        /// <summary>
        /// Удалить
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.Delete();
        }

        /// <summary>
        /// Удалить из библиотеки
        /// </summary>
        private void DeleteFromLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.Delete();
        }

        /// <summary>
        /// Добавить в библиотеку
        /// </summary>
        private void AddToLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.AddToLibrary();
        }

        /// <summary>
        /// Посетить веб-сайт
        /// </summary>
        private void VisitWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.GoToWebsite();
        }

        private void PART_LogoBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LogoButtonCommand.Execute(LogoButtonCommandParameter);
        }


        private void OnInstanceDeleted(object obj) 
        {
            PlayDeleteAnimation();
            _model.DeletedEvent -= OnInstanceDeleted;
        }

        private void PlayDeleteAnimation() 
        {
            var doubleAnim = new DoubleAnimation()
            {
                From = 1,
                To = 0.9,
                Duration = TimeSpan.FromSeconds(0.20)
            };

            doubleAnim.Completed += (e, e1) =>
            {
                var doubleAnim1 = new DoubleAnimation()
                {
                    From = this.ActualHeight,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.10)
                };
                this.BeginAnimation(HeightProperty, doubleAnim1);
            };

            this.BeginAnimation(OpacityProperty, doubleAnim);
        }

        #endregion Lower Button Click

        private void UpdateIndicatorMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_model.State == InstanceState.Default)
                _model.Update();
        }
    }
}
