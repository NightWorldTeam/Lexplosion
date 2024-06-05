using Lexplosion.WPF.NewInterface.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Lexplosion.WPF.NewInterface.Mvvm.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private string[] TestItems { get; } = [
            "Adventure and RPG",
            "Combat / PvP",
            "Exploration",
            "Extra Large",
            "FTB Official Pack",
            "Hardcore",
            "Magic",
            "Map Based",
            "Mini Game",
            "Multiplayer",
            "Quests",
            "Sci - Fi",
            "Skyblock",
            "Small / Light",
            "Tech",
            "Vanilla + ",
        ];

        private readonly CollectionViewSource _collectionViewSource = new();

        private readonly IEnumerable SelectedItems = new List<string>();

        public TestWindow()
        {
            InitializeComponent();
            multiSelectComboBox.ItemsSource = TestItems;
            ListBox1.ItemsSource = TestItems;
            _collectionViewSource.Source = TestItems;
            CategoriesList.SearchInCollection = (str) => 
            {
                _collectionViewSource.View.Filter += (item) => ((str.Length == 0 ||
                    (item as string).IndexOf(str, System.StringComparison.InvariantCultureIgnoreCase) > -1)); //&& !CategoriesList.SelectedItems.Contains(item));
            };
            //SelectedItems = CategoriesList.SelectedItems;
            CategoriesList.ItemsSource = _collectionViewSource.View;
        }

        private void ListBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            foreach (var s in e.AddedItems)
                Console.WriteLine(s.GetType());
        }
    }
}
