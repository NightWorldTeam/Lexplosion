using System.Windows.Controls;

namespace Lexplosion.Gui.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthView.xaml
    /// </summary>
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
        }

        private void NicknameTextBlock_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //var constTextBlockName = "NicknameTextBlock";

            //var thisTextBlock = (TextBlock)sender;
            //var newTextBlock = new TextBlock()
            //{
            //    Foreground = thisTextBlock.Foreground,
            //    FontSize = thisTextBlock.FontSize,
            //    Name = thisTextBlock.Name.Contains("_1") ? constTextBlockName + "_1" : constTextBlockName + "_2",
            //    VerticalAlignment = System.Windows.VerticalAlignment.Center,
            //};
            //newTextBlock.Loaded += NicknameTextBlock_Loaded;

            //if (thisTextBlock.Text.Length > 8)
            //{
            //    newTextBlock.Margin = new System.Windows.Thickness(5, 10, 0, 0);
            //    newTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            //    HeaderGrid.Children.Remove(thisTextBlock);
            //    HeaderStackPanel.Children.Add(newTextBlock);

            //}
            //else
            //{
            //    newTextBlock.Margin = new System.Windows.Thickness(5, 5, 0, 0);
            //    newTextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //}
        }
    }
}
