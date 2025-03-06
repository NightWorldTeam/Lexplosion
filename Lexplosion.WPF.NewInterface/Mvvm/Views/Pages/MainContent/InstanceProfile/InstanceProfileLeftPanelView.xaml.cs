using Lexplosion.WPF.NewInterface.Controls;
using Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Pages.MainContent.InstanceProfile
{
    /// <summary>
    /// Логика взаимодействия для InstanceProfileLeftPanelView.xaml
    /// </summary>
    public partial class InstanceProfileLeftPanelView : UserControl
    {
        private InstanceProfileLeftPanelViewModel _model;

        public InstanceProfileLeftPanelView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _model = (InstanceProfileLeftPanelViewModel)DataContext;
            SetVisitButtonIconAndText();
        }

        private void SetVisitButtonIconAndText()
        {
            VisitWebsiteButton.SetResourceReference(AdvancedButton.IconDataProperty, $"PD{_model.InstanceModel.Source}");
            VisitWebsiteButton.SetResourceReference(AdvancedButton.TextProperty, $"Visit{_model.InstanceModel.Source}");
        }

        #region Lower Buttons Click


        /// <summary>
        /// Отмена скачивания
        /// </summary>
        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.InstanceModel.CancelDownload();
        }

        /// <summary>
        /// Открытие папки
        /// </summary>
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.InstanceModel.OpenFolder();
        }

        /// <summary>
        /// Открытие меню экспорта
        /// </summary>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.InstanceModel.Export();
        }

        /// <summary>
        /// Удалить
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.DeleteInstance();
        }

        /// <summary>
        /// Удалить из библиотеки
        /// </summary>
        private void DeleteFromLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.DeleteInstance();
        }

        /// <summary>
        /// Добавить в библиотеку
        /// </summary>
        private void AddToLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.InstanceModel.AddToLibrary();
        }

        /// <summary>
        /// Посетить веб-сайт
        /// </summary>
        private void VisitWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            PART_DropDownMenu.IsOpen = false;
            _model.InstanceModel.GoToWebsite();
        }


        #endregion Lower Button Click
    }
}
