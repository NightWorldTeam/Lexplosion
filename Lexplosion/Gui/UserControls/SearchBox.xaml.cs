using Lexplosion.Gui.Pages.MW;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lexplosion.Gui.UserControls
{
    /// <summary>
    /// Логика взаимодействия для SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public static readonly SolidColorBrush MouseDownColor = new SolidColorBrush(Color.FromArgb(255, 44, 153, 194));
        public static readonly SolidColorBrush MouseUpColor = new SolidColorBrush(Color.FromArgb(255, 19, 21, 19));

        private InstanceContainerPage page;

        public SearchBox(InstanceContainerPage _page)
        {
            InitializeComponent();
            page = _page;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e) 
        {
			var last_request = "";
			if (page.InstanceGrid.Children.Count > 2) page.InstanceGrid.Children.RemoveRange(2, 10);

			if (page.InstanceGrid.RowDefinitions.Count > 2)
				page.InstanceGrid.RowDefinitions.RemoveRange(1, page.InstanceGrid.RowDefinitions.Count - 1);

            InstanceType selectedInstanceType;

            if (SelectInstanceTypeBox.SelectedIndex == 0) selectedInstanceType = InstanceType.Nightworld;
            else selectedInstanceType = InstanceType.Curseforge;

            if (SearchTextBox.Text.Length != 0)
            {
                //if (last_request != SearchBox.Text || last_request == "") { 
                last_request = SearchTextBox.Text;
                page._isInitializeInstance = false;
                //TODO: Вызывать функцию в LeftSideMenu, что вероянее всего уберёт задержку между auth и main window, а также уберёт перевызов из других страниц...
                List<OutsideInstance> instances = ManageLogic.GetOutsideInstances(selectedInstanceType, 10, 0, ModpacksCategories.All, SearchTextBox.Text); ;
                if (instances.Count > 0)
                    for (int j = 0; j < instances.ToArray().Length; j++)
                    {
                        page.BuildInstanceForm(instances[j].Id.ToString(), j + 1,
                            new Uri(instances[j].MainImageUrl),
                            instances[j].Name,
                            instances[j].Author, 
                            instances[j].Description,
                            instances[j].Categories);
                        page.LoadingLable.Visibility = Visibility.Collapsed;
                    }
                else
                {
                    page.LoadingLable.Text = "Результаты не найдены.";
                    page.LoadingLable.Visibility = Visibility.Visible;
                }
            }
            else
            {
                page.LoadingLable.Text = "Идёт загрузка. Пожалуйста подождите...";
                page.GetInitializeInstance();
                page.LoadingLable.Visibility = Visibility.Visible;
            }
        }
    }
}
