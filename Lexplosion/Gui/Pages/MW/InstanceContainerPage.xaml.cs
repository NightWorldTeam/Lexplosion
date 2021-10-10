using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Gui.Pages.MW
{
    /// <summary>
    /// Interaction logic for InstanceContainerPage.xaml
    /// </summary>
    public partial class InstanceContainerPage : Page
	{
		public static InstanceContainerPage obj = null;

		private MainWindow _mainWindow;
		public bool _isInitializeInstance = false;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();
			SearchBox searchBox = new SearchBox(this)
			{
				Margin = new Thickness(0, 14, 0, 0),
				Width = 400,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			obj = this;
			_mainWindow = mainWindow;
			InstanceGrid.Children.Add(searchBox);
			GetInitializeInstance();
		}

		public async void GetInitializeInstance()
		{
			await Task.Run(() => InitializeInstance());
			_isInitializeInstance = true;
			LoadingLable.Visibility = Visibility.Collapsed;
		}

		private void InitializeInstance()
		{
			List<OutsideInstance> instances = ManageLogic.GetOutsideInstances(InstanceType.Curseforge, 5, 0, ModpacksCategories.All);
			for (int j = 0; j < instances.ToArray().Length; j++)
			{
				// TODO: размер curseforgeInstances[j].attachments или curseforgeInstances[j].authors может быть равен нулю и тогда будет исключение
				// TODO: в curseforgeInstances[j].attachments нужно брать не первый элемент, а тот у котрого isDefault стоит на true
				BuildInstanceForm(instances[j].Id, j + 1,
					new Uri(instances[j].MainImageUrl),
					instances[j].Name,
					instances[j].Author,
					instances[j].Description,
					instances[j].Categories);
			}
		}

		// TODO: Надо сделать констуктор модпака(ака либо загрузить либо по кнопкам), также сделать чёт типо формы и предпросмотр как это будет выглядить.

		public void BuildInstanceForm(string instanceId, int row, Uri logoPath, string title, string author, string overview, List<string> tags)
		{
			this.Dispatcher.Invoke(() =>
			{
				InstanceGrid.RowDefinitions.Add(GetRowDefinition());
				UserControls.InstanceForm instanceForm = new UserControls.InstanceForm(
					_mainWindow, title, "", author, overview, Int32.Parse(instanceId), logoPath, tags, false, false
				);
				// Добавление в Столбики и Колноки в форме.
				Grid.SetRow(instanceForm, row);
				InstanceGrid.Children.Add(instanceForm);
			});
		}

		private RowDefinition GetRowDefinition()
		{
			RowDefinition rowDefinition = new RowDefinition()
			{
				Height = new GridLength(150, GridUnitType.Pixel)
			};
			return rowDefinition;
		}
	}
}
