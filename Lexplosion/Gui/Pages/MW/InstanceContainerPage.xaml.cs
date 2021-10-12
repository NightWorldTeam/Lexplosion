using Lexplosion.Gui.UserControls;
using Lexplosion.Gui.Windows;
using Lexplosion.Logic.Management;
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
		private SearchBox searchBox;
		private int pageSize = 10;

		public InstanceContainerPage(MainWindow mainWindow)
		{
			InitializeComponent();
			searchBox = new SearchBox(this)
			{
				Margin = new Thickness(0, 14, 0, 0),
				Width = 400,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			obj = this;
			_mainWindow = mainWindow;
			InstanceGrid.Children.Add(searchBox);
			GetInitializeInstance(InstanceSource.Curseforge);
		}

		public async void GetInitializeInstance(InstanceSource instanceSource)
		{
			await Task.Run(() => InitializeInstance(instanceSource));
			_isInitializeInstance = true;
			ChangeLoadingLabel("", Visibility.Collapsed);
		}

		private void InitializeInstance(InstanceSource instanceSource)
		{
			List<OutsideInstance> instances = ManageLogic.GetOutsideInstances(instanceSource, pageSize, 0, ModpacksCategories.All);
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
					_mainWindow, title, "", author, overview, instanceId, logoPath, tags, false, false
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

		public void SearchInstances()
		{
			var searchBoxTextLength = searchBox.SearchTextBox.Text.Length;
			var sourceBoxSelectedIndex = searchBox.SourceBox.SelectedIndex;
			var searchBoxText = searchBox.SearchTextBox.Text;
			var loadingLableText = LoadingLabel.Text;
			var selectedInstanceSource = (InstanceSource)sourceBoxSelectedIndex;

			if (InstanceGrid.Children.Count > 2) InstanceGrid.Children.RemoveRange(2, 10);

			if (InstanceGrid.RowDefinitions.Count > 2)
				InstanceGrid.RowDefinitions.RemoveRange(1, InstanceGrid.RowDefinitions.Count - 1);

			ChangeLoadingLabel("Идёт загрузка. Пожалуйста подождите...", Visibility.Visible);

			Lexplosion.Run.ThreadRun(delegate ()
			{
				if (searchBoxTextLength != 0 || sourceBoxSelectedIndex != searchBox.LastSelectedIndex)
				{
					_isInitializeInstance = false;
					//TODO: Вызывать функцию в LeftSideMenu, что вероянее всего уберёт задержку между auth и main window, а также уберёт перевызов из других страниц...
					List<OutsideInstance> instances = ManageLogic.GetOutsideInstances(selectedInstanceSource, pageSize, 0, ModpacksCategories.All, searchBoxText); ;
					if (instances.Count > 0)
					{
						for (int j = 0; j < instances.ToArray().Length; j++)
						{
							BuildInstanceForm(instances[j].Id.ToString(), j + 1,
								new Uri(instances[j].MainImageUrl),
								instances[j].Name,
								instances[j].Author,
								instances[j].Description,
								instances[j].Categories);
							ChangeLoadingLabel(loadingLableText, Visibility.Collapsed);
						}
					}
					else
					{
						ChangeLoadingLabel("Результаты не найдены.", Visibility.Visible);
					}

					searchBox.LastRequest = searchBoxText;
					searchBox.LastSelectedIndex = sourceBoxSelectedIndex;
				}
				else
				{
					GetInitializeInstance(selectedInstanceSource); ;
					ChangeLoadingLabel("Идёт загрузка. Пожалуйста подождите...", Visibility.Visible);
				}
			});
		}

		private void ChangeLoadingLabel(string content, Visibility visibility) 
		{
			this.Dispatcher.Invoke(() => { 
				LoadingLabel.Text = content;
				LoadingLabel.Visibility = visibility;
			});
		}
	}
}
