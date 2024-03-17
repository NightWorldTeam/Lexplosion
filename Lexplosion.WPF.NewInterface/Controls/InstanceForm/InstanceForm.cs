using Lexplosion.Logic.Management;
using Lexplosion.Logic.Objects;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel.InstanceModelBase;
using static System.Windows.Forms.AxHost;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_LOGOBLOCK, Type = typeof(Grid))]
    [TemplatePart(Name = PART_LOGO, Type = typeof(Border))]
    [TemplatePart(Name = PART_NAME, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_AUTHOR, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_SHORT_DESCRIPTION, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_TAGS_PANEL, Type = typeof(ItemsControl))]
    [TemplatePart(Name = PART_MAIN_ACTION_BUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_DROPDOWNMENU, Type = typeof(DropdownMenu))]
    [TemplatePart(Name = PART_DROPDOWNMENU_CONTENT, Type = typeof(ItemsControl))]

    [TemplatePart(Name = PART_DROPDOWNMENU_CONTENT, Type = typeof(ProgressBar))]
    [TemplatePart(Name = PART_MAIN_BUTTON_PERCENTAGE, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_MAIN_BUTTON_PERCENTAGE_ACTIVITY_COLOR, Type = typeof(TextBlock))]
    public sealed partial class InstanceForm : Control
    {
        private const string PART_LOGOBLOCK = "PART_LogoBlock";

        // Overview Info
        private const string PART_LOGO = "PART_Logo";
        private const string PART_NAME = "PART_Name";
        private const string PART_AUTHOR = "PART_Author";
        private const string PART_SHORT_DESCRIPTION = "PART_Description";
        private const string PART_TAGS_PANEL = "PART_TagsPanel";

        // Buttons
        private const string PART_MAIN_ACTION_BUTTON = "PART_MainActionButton";
        private const string PART_DROPDOWNMENU = "PART_DropDownMenu";
        private const string PART_DROPDOWNMENU_CONTENT = "PART_DropdownMenuContent";

        // Download Info
        private const string PART_PROGRESSBAR = "PART_ProgressBar";
        private const string PART_FILESCOUNT_LABEL = "PART_FilesCountLabel";

        // Icon Keys
        private const string IK_PLAY = "Play";
        private const string IK_DOWNLOAD = "Download";
        private const string IK_CLOSE = "Close";
        private const string IK_LANG = "Lang";
        private const string IK_DELETE = "Delete";
        private const string IK_EXPORT = "Export";
        private const string IK_OPENFOLDER = "Folder";
        private const string IK_DLC = "Dlc";
        private const string IK_ADD_TO_LIBRARY = "AddToLibrary";
        private const string IK_REMOVE_FROM_LIBRARY = "RemoveFromLibrary";

        private const string PART_MAIN_BUTTON_PERCENTAGE = "Percentage";
        private const string PART_MAIN_BUTTON_PERCENTAGE_ACTIVITY_COLOR = "PercentageActivityColor";


        /// Presentation Info
        private Grid _logoGridBlock;
        private Border _logoBorder;
        private TextBlock _nameTextBlock;
        private TextBlock _authorTextBlock;
        private TextBlock _shortDescriptionTextBlock;
        private ItemsControl _tagsPanel;

        private Button _mainActionButton;
        private DropdownMenu _dropdownMenu;
        private ItemsControl _dropdownMenuItemsControl;

        private ProgressBar _progressBar;
        private TextBlock _filesCountLabel;

        private Path _mainActionButtonPath;
        private Path _mainActionButtonPathHover;

        private TextBlock _mainActionButtonPercentage;
        private TextBlock _mainActionButtonPercentageHover;

        /// <summary>
        /// Вызывает когда метод OnApplyTemplate завершает работу.
        /// </summary>
        public event Action ApplyTemplateExecuted;

        // -- State -- //

        public event Action LowerButtonClicked;
        
        // -- State -- //


        private ObservableCollection<LowerMenuButton> _lowerMenuButtons = new ObservableCollection<LowerMenuButton>();
        public IEnumerable<LowerMenuButton> LowerMenuButtons { get => _lowerMenuButtons; }


        #region Dependency Properties


        public static readonly DependencyProperty InstanceModelProperty
                = DependencyProperty.Register(nameof(InstanceModel), typeof(InstanceModelBase), typeof(InstanceForm),
                    new FrameworkPropertyMetadata(propertyChangedCallback: OnInstanceModelChanged));

        public static readonly DependencyProperty LogoButtonCommandProperty
            = DependencyProperty.Register(nameof(LogoButtonCommand), typeof(ICommand), typeof(InstanceForm),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty LogoButtonCommandParameterProperty
            = DependencyProperty.Register(nameof(LogoButtonCommandParameter), typeof(object), typeof(InstanceForm),
                new FrameworkPropertyMetadata(defaultValue: (object)null));

        public InstanceModelBase InstanceModel
        {
            get => (InstanceModelBase)GetValue(InstanceModelProperty);
            set => SetValue(InstanceModelProperty, value);
        }

        public ICommand LogoButtonCommand
        {
            get => (ICommand)GetValue(LogoButtonCommandProperty);
            set => SetValue(LogoButtonCommandProperty, value);
        }

        public object LogoButtonCommandParameter
        {
            get => (object)GetValue(LogoButtonCommandParameterProperty);
            set => SetValue(LogoButtonCommandParameterProperty, value);
        }


        #endregion Dependency Properties


        #region Properties


        private bool _isProgressActive;
        /// <summary>
        /// Активен ли хоть один процесс.
        /// </summary>
        public bool IsProcessActive
        {
            get => _isProgressActive; private set
            {
                _isProgressActive = value;
                OnProcessActive();
            }
        }

        private bool _isDownloading;
        /// <summary>
        /// Скачивается ли клиент игры сейчас.
        /// </summary>
        public bool IsDownloading
        {
            get => _isDownloading; private set
            {
                _isDownloading = value;
                OnDownloadingChanged();
            }
        }


        #endregion Properties


        #region Constructors


        static InstanceForm()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InstanceForm), new FrameworkPropertyMetadata(typeof(InstanceForm)));
        }

        public InstanceForm()
        {
            Loaded += (object sender, RoutedEventArgs e) => 
            {
                IsProcessActive = false;
                IsDownloading = false;
                OnInstanceModelStateChanged();
            };
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            _logoGridBlock = Template.FindName(PART_LOGOBLOCK, this) as Grid;
            _logoBorder = Template.FindName(PART_LOGO, this) as Border;
            _nameTextBlock = Template.FindName(PART_NAME, this) as TextBlock;
            _authorTextBlock = Template.FindName(PART_AUTHOR, this) as TextBlock;
            _shortDescriptionTextBlock = Template.FindName(PART_SHORT_DESCRIPTION, this) as TextBlock;
            _tagsPanel = Template.FindName(PART_TAGS_PANEL, this) as ItemsControl;
            _dropdownMenu = Template.FindName(PART_DROPDOWNMENU, this) as DropdownMenu;
            _dropdownMenuItemsControl = Template.FindName(PART_DROPDOWNMENU_CONTENT, this) as ItemsControl;
            _mainActionButton = Template.FindName(PART_MAIN_ACTION_BUTTON, this) as Button;

            // donwload progress
            _progressBar = Template.FindName(PART_PROGRESSBAR, this) as ProgressBar;
            _filesCountLabel = Template.FindName(PART_FILESCOUNT_LABEL, this) as TextBlock;

            if (_logoGridBlock != null)
            {
                _logoGridBlock.MouseLeftButtonDown += _logoGridBlock_MouseLeftButtonDown;
            }

            if (_mainActionButton != null)
            {
                _mainActionButton.Click += _mainActionButton_Click;
            }

            if (_dropdownMenu != null)
            {
                LowerButtonClicked += () => _dropdownMenu.IsOpen = false;
            }

            if (_dropdownMenuItemsControl != null)
            {
                _dropdownMenuItemsControl.ItemsSource = LowerMenuButtons;

                UpdateLowerButtons();
            }

            UpdateAllFields(InstanceModel);

            InstanceModel.LogoChanged += () =>
            {
                App.Current.Dispatcher.Invoke((() => 
                {
                    SetLogo(InstanceModel.Logo);
                }));
            };

            InstanceModel.StateChanged += OnInstanceModelStateChanged;

            InstanceModel.DeletedEvent += (s) =>
            {
                DeleteAnimation();
            };

            IsProcessActive = false;
            IsDownloading = false;

            InstanceModel.DownloadProgressChanged += OnDownloadProcessChanged;
            InstanceModel.DownloadComplited += OnDownloadCompleted;

            InstanceModel.GameLaunched += OnGameLaunched;
            InstanceModel.GameLaunchCompleted += OnLaunchedCompleted;
            InstanceModel.GameClosed += OnGameClosed;
            InstanceModel.DownloadCanceled += OnDownloadCanceled;

            base.OnApplyTemplate();
            ApplyTemplateExecuted?.Invoke();
            Runtime.DebugWrite("OnApplyTemplate");
        }

        private void OnInstanceModelStateChanged()
        {
            // Обновляем список доступных lower buttons.
            App.Current.Dispatcher.Invoke(() =>
            {
                UpdateLowerButtons();

                switch (InstanceModel.State)
                {
                    case InstanceState.Default:
                        {
                            IsDownloading = false;
                            _mainActionButton.IsEnabled = true;
                            SetShortDescription(InstanceModel.Description);
                            if (InstanceModel.IsInstalled)
                                ChangeMainActionButtonIcon(IK_PLAY);
                            else
                                ChangeMainActionButtonIcon(IK_DOWNLOAD);
                            break;
                        }
                    case InstanceState.Preparing:
                            {
                                IsDownloading = false;
                                IsProcessActive = true;
                                PrepareVisualState();
                                break;
                            }
                    case InstanceState.Downloading:
                        {
                            IsProcessActive = true;
                            IsDownloading = true;
                            ChangeMainActionButtonIcon(string.Empty);
                            _mainActionButton.IsEnabled = true;
                            SetMainActionButtonPercentageValue("0");
                            break;
                        }
                    case InstanceState.DownloadCanceling:
                        {
                            IsDownloading = false;
                            IsProcessActive = true;
                            // TODO: Translate
                            SetShortDescription("Отмена скачивания...");
                            _mainActionButton.IsEnabled = false;
                            // убираем иконку
                            ChangeMainActionButtonIcon(string.Empty);
                            // убираем проценты
                            SetMainActionButtonTextValue();
                            break;
                        }
                    case InstanceState.Launching:
                        {
                            ChangeMainActionButtonIcon(IK_CLOSE);
                            _mainActionButton.IsEnabled = true;
                            IsDownloading = false;
                            break;
                        }
                    case InstanceState.Running:
                        {
                            ChangeMainActionButtonIcon(IK_CLOSE);
                            IsDownloading = false;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            });
        }

        /// <summary>
        /// Переводит массив byte в картинку в формате BitmapImage
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static BitmapImage ToImage(string name, byte[] imageBytes)
        {
            if (imageBytes is null || imageBytes.Length == 0)
                return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png"));

            Runtime.DebugWrite(name + " " + imageBytes.Length);

            using (var stream = new System.IO.MemoryStream(imageBytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }


        #endregion Public & Protected Methods


        #region Private Methods


        #region Presentation Info


        /// <summary>
        /// Устанавливает значение для поле формы Name
        /// </summary>
        /// <param name="value">Значение</param>
        private void SetName(string value)
        {
            if (_nameTextBlock == null)
                return;

            // todo: IMPORTANT сделать привязку свойств aka binding
            //Binding binding = new Binding();
            //binding.Source = InstanceModel;
            //binding.Path = new PropertyPath("Text");
            //_nameTextBlock.SetBinding(TextBlock.TextProperty, binding);

            _nameTextBlock.Text = InstanceModel.Name;
        }

        /// <summary>
        /// Устанавливает значение для поле формы ShortDescription
        /// </summary>
        /// <param name="value">Значение</param>
        private void SetShortDescription(string value)
        {
            if (_shortDescriptionTextBlock == null)
                return;

            App.Current.Dispatcher.Invoke(() => 
            { 
                _shortDescriptionTextBlock.Text = value;
            });
        }

        /// <summary>
        /// Устанавливает значение для поле формы Author
        /// </summary>
        /// <param name="value">Значение</param>
        private void SetAuthor(string value)
        {
            if (_authorTextBlock == null)
                return;

            _authorTextBlock.Text = "by " + value;
        }

        /// <summary>
        /// Устанавливает список тэгов для поле формы TagsPanel
        /// </summary>
        /// <param name="value">Коллекция с тэгами</param>
        private void SetCategories(IEnumerable<object> objects)
        {
            if (_tagsPanel == null)
                return;

            List<object> c = new List<object>();

            if (objects != null)
                c = new List<object>(objects);

            c.Insert(0, new SimpleCategory("", InstanceModel.GameVersion?.ToString()));

            //c.Sort((i1, i2) => i1.ToString().Length.CompareTo(i2.ToString().Length));

            _tagsPanel.ItemsSource = c;
        }

        /// <summary>
        /// Устанавливает значение для поле формы Logo
        /// </summary>
        /// <param name="value">Массив байт картинки, которую нужно установить</param>
        private void SetLogo(byte[] logo)
        {
            if (_logoBorder == null)
                return;

            _logoBorder.Background = new ImageBrush()
            {
                ImageSource = ToImage(InstanceModel?.Name, logo)
            };
        }

        /// <summary>
        /// Обновляет все поля с презентационной информацияей используя информацию InstanceModelBase.
        /// </summary>
        /// <param name="model">InstanceModelBase информацию которого мы используем</param>
        private void UpdateAllFields(InstanceModelBase model)
        {
            App.Current.Dispatcher?.Invoke(() => 
            { 
                SetName(model.Name);
                SetShortDescription(model.Summary);
                SetCategories(model.Tags);
                SetAuthor(model.Author);
                SetLogo(model.Logo);
            });
        }


        #endregion Presentation Info


        private static void OnInstanceModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instanceForm = d as InstanceForm;
            var instanceModelBase = ((InstanceModelBase)e.NewValue);

            //instanceModelBase.NameChanged += instanceForm.SetName;

            instanceForm.UpdateAllFields(instanceModelBase);
        }

        /// <summary>
        /// Отрабатывает при нажатии на Logo Grid, выполняет Command если оно было задано.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _logoGridBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogoButtonCommand == null) return;

            LogoButtonCommand.Execute(LogoButtonCommandParameter);
        }

        /// <summary>
        /// Отменяет скачивание клиента игры.
        /// </summary>
        private void CancelDownload() 
        {
            InstanceModel.CancelDownload();
        }

        /// <summary>
        /// Обновляет список lowerbutton, в зависимости от состояния InstanceModel.
        /// </summary>
        private void UpdateLowerButtons()
        {
            _lowerMenuButtons.Clear();

            if (InstanceModel.State == InstanceState.Downloading)
            {
                _lowerMenuButtons.Add(new LowerMenuButton(0, "Cancel", "CancelDownload", CancelDownload, LowerButtonClicked));
            }

            if (InstanceModel.Source != InstanceSource.Local)
            {
                _lowerMenuButtons.Add(new LowerMenuButton(0, InstanceModel.Source.ToString(), "Visit" + InstanceModel.Source.ToString(), InstanceModel.GoToWebsite, LowerButtonClicked));
            }

            if (!InstanceModel.IsInstalled && !InstanceModel.InLibrary)
            {
                _lowerMenuButtons.Add(new LowerMenuButton(1, "AddToLibrary", "AddToLibrary", InstanceModel.AddToLibrary, LowerButtonClicked));
            }

            if (InstanceModel.InLibrary)
            {
                _lowerMenuButtons.Add(new LowerMenuButton(2, "Folder", "OpenFolder", InstanceModel.OpenFolder, LowerButtonClicked));
                // TODO: Сделать возможность экспорта, для только созданного клиента. Так как, человек может вручную накидать модов или установить что-то.
                if (InstanceModel.IsInstalled)
                {
                    _lowerMenuButtons.Add(new LowerMenuButton(3, "Export", "Export", InstanceModel.Export, LowerButtonClicked));
                }
                _lowerMenuButtons.Add(new LowerMenuButton(4, "Addons", "Addons", () => { }, LowerButtonClicked));
            }

            if (!InstanceModel.IsInstalled && InstanceModel.InLibrary)
            {
                _lowerMenuButtons.Add(new LowerMenuButton(5, "Delete", "RemoveFromLibrary", InstanceModel.Delete, LowerButtonClicked));
            }
            else if (InstanceModel.IsInstalled)
            {
                _lowerMenuButtons.Add(new LowerMenuButton(5, "Delete", "DeleteInstance", InstanceModel.Delete, LowerButtonClicked));
            }
        }

        /// <summary>
        /// Вызывается при клике на MainButtonю
        /// </summary>
        /// <param name="sender">Объект кнопки</param>
        /// <param name="e">Аргументы события</param>
        private void _mainActionButton_Click(object sender, RoutedEventArgs e)
        {
                switch (InstanceModel.State)
                {
                    case InstanceState.Default:
                        {
                            if (InstanceModel.IsInstalled)
                            {
                                InstanceModel.Run();
                            }
                            else
                            {
                                InstanceModel.Download();
                            }
                            break;
                        }
                    case InstanceState.Downloading:
                        {
                            //Open Modal Windows with instance about downloading.
                            break;
                        }
                    case InstanceState.Launching:
                        {
                            StopProcessVisual();
                            break;
                        }
                    case InstanceState.Running:
                        {
                            StopProcessVisual();
                            break;
                        }
                    default:
                        break;
                }
            }

        /// <summary>
        /// Включает видимость TextBlock для вывода статистики, по количеству всех и скаченных файлов.
        /// </summary>
        private void ShowProcessFilesCount()
        {
            _filesCountLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Выключает видимость TextBlock для вывода статистики, по количеству всех и скаченных файлов.
        /// </summary>
        private void CollabsedProcessFilesCount()
        {
            _filesCountLabel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Устанавливает значение в TextBlock для вывода статистики, по количеству всех и скаченных файлов.
        /// </summary>
        /// <param name="content">Строка которую нужно отобразить</param>
        private void SetProcessFilesCount(string content)
        {
            _filesCountLabel.Text = content;
        }

        /// <summary>
        /// Задает значение полю с коротким описание, по ключу
        /// </summary>
        /// <param name="key">Ключ текста</param>
        /// <exception cref="Exception">Если, ключ отсутсвует в словаре приложения.</exception>
        private void SetProcessDescriptionKey(string key, params object[] args)
        {
            if (App.Current.Resources[key] == null)
                throw new Exception($"Key - {key} is null!");

            Lexplosion.WPF.NewInterface.Extensions.TextBlock.SetTextByKey(_shortDescriptionTextBlock, key);

            //var fullArgs = new object[args.Length + 1];
            //args.CopyTo(fullArgs, 1);
            //fullArgs[0] = _shortDescriptionTextBlock.Text;
            //_shortDescriptionTextBlock.Text = new StringFormatConverter().Convert(fullArgs, typeof(string), null, null).ToString();
        }

        /// <summary>
        /// Возвращает визуальное состояние формы на стандартное состояние.
        /// </summary>
        private void StopProcessVisual()
        {
            Runtime.DebugWrite("Close clicked");
            InstanceModel.Close();
            IsDownloading = false;
            IsProcessActive = false;
        }

        /// <summary>
        /// Изменяет визуальное состояние формы на состояние подготовки. 
        /// </summary>
        private void PrepareVisualState()
        {
            ChangeMainActionButtonIcon(string.Empty);
            SetMainActionButtonPercentageValue("0");
            if (InstanceModel.IsInstalled)
                SetShortDescription("Проверка целостности игровых файлов...");
            else
                SetShortDescription("Подготовка к скачиванию клиента...");
        }

        /// <summary>
        /// Изменяет иконку в mainbutton на iconKey.
        /// </summary>
        /// <param name="iconKey">Ключ иконки которую нужно отбразить</param>
        private void ChangeMainActionButtonIcon(string iconKey)
        {
            if (_mainActionButtonPath == null)
            {
                _mainActionButtonPath = _mainActionButton.Template.FindName("Icon", _mainActionButton) as Path;
                _mainActionButtonPathHover = _mainActionButton.Template.FindName("IconActivityColor", _mainActionButton) as Path;
            }

            Extensions.Path.SetStringKeyData(_mainActionButtonPath, iconKey);
            Extensions.Path.SetStringKeyData(_mainActionButtonPathHover, iconKey);
        }

        /// <summary>
        /// Изменяет значение поля для процентов в mainbutton на строку value%.
        /// </summary>
        /// <param name="value"></param>
        private void SetMainActionButtonPercentageValue(string value)
        {
            SetMainActionButtonTextValue($"{value}%");
        }

        /// <summary>
        /// Изменяет значение в поле для процентов в mainbutton на значение value.
        /// </summary>
        /// <param name="value">Значение которое нужно отобразить</param>
        private void SetMainActionButtonTextValue(string value = "")
        {
            if (_mainActionButtonPercentage == null)
            {
                _mainActionButtonPercentage = _mainActionButton.Template.FindName(PART_MAIN_BUTTON_PERCENTAGE, _mainActionButton) as TextBlock;
                _mainActionButtonPercentageHover = _mainActionButton.Template.FindName(PART_MAIN_BUTTON_PERCENTAGE_ACTIVITY_COLOR, _mainActionButton) as TextBlock;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                _mainActionButtonPercentage.Text = value;
                _mainActionButtonPercentageHover.Text = value;
            });
        }


        ///
        /// <!-- Download Handlers --> ///
        ///


        /// <summary>
        /// Вызывается каждый раз когда, информация о скачивании меняется.
        /// Изменяет визуальное состояние формы, включается progressbar, показывает проценты и т.д
        /// </summary>
        /// <param name="stageType">Стадия скачивания</param>
        /// <param name="progressHandlerArguments">Информация об изменении скачивания.</param>
        private void OnDownloadProcessChanged(StageType stageType, ProgressHandlerArguments progressHandlerArguments)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!IsProcessActive)
                    IsProcessActive = true;

                switch (stageType)
                {
                    case StageType.Prepare:
                        {
                            PrepareVisualState();
                            _progressBar.IsIndeterminate = true;

                            if (IsDownloading)
                                IsDownloading = false;
                        }
                        break;
                    case StageType.Java:
                        {
                            if (!IsDownloading)
                                IsDownloading = true;
                            SetProcessDescriptionKey("JavaInstalling");
                            ShowProcessFilesCount();
                            SetProcessFilesCount($"Скачено {progressHandlerArguments.FilesCount}/{progressHandlerArguments.TotalFilesCount}");
                            if (_progressBar.IsIndeterminate)
                                _progressBar.IsIndeterminate = false;
                            _progressBar.Value = progressHandlerArguments.Procents;
                        }
                        break;
                    case StageType.Client:
                        {
                            if (!IsDownloading)
                                IsDownloading = true;
                            // Идёт скачинвание. Этап 1/1;
                            SetProcessDescriptionKey("InstanceDownloading", progressHandlerArguments.Stage, progressHandlerArguments.StagesCount);
                            SetProcessFilesCount($"Скачено {progressHandlerArguments.FilesCount}/{progressHandlerArguments.TotalFilesCount}");
                            if (_progressBar.IsIndeterminate)
                                _progressBar.IsIndeterminate = false;
                            _progressBar.Value = progressHandlerArguments.Procents;
                            SetMainActionButtonPercentageValue(progressHandlerArguments.Procents.ToString());
                        }
                        break;
                }
            });
        }

        /// <summary>
        /// Вызывается, при окончании скачивания.
        /// Если данный метод был вызван и isGameRunning = true, значит предыдущей стадией была Проверка Целостности Файлов.
        /// </summary>
        /// <param name="result">Результат с которым завершилось скачивание</param>
        /// <param name="downloadErrors">Список ошибок скачивания, если они есть</param>
        /// <param name="isGameRunning">Запускается ли игра</param>
        private void OnDownloadCompleted(InstanceInit result, IEnumerable<string> downloadErrors, bool isGameRunning)
        {
            Runtime.DebugWrite("Instance Form - Download Completed");
            App.Current.Dispatcher.Invoke(() =>
            {
                SetMainActionButtonTextValue();
                _progressBar.IsIndeterminate = true;
                if (isGameRunning)
                {
                    IsDownloading = false;
                    _mainActionButton.IsEnabled = true;
                    SetShortDescription("Идет запуск игры...");
                    return;
                }

                IsDownloading = false;
                IsProcessActive = false;
                SetShortDescription(InstanceModel.Summary);
            });
        }

        /// <summary>
        /// Вызывается, когда было запущенно отмена скачинвания клиента.
        /// </summary>
        private void OnDownloadCanceled()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                IsProcessActive = false;
                IsDownloading = false;
            });
        }


        ///
        /// <!-- Launch Handlers --> ///
        ///


        /// <summary>
        /// Вызывается, когда начался запуск клиента игры.
        /// </summary>
        private void OnGameLaunched()
        {
            //State = InstanceFormState.Launching;
            Runtime.DebugWrite("Started");
        }

        /// <summary>
        /// Вызывается, когда завершается запуск клиента игры.
        /// </summary>
        /// <param name="isSuccessful">Успешен ли запуск</param>
        private void OnLaunchedCompleted(bool isSuccessful)
        {
            App.Current.Dispatcher.Invoke(() => {
                SetShortDescription(InstanceModel.Summary);
                IsProcessActive = false;
                _tagsPanel.Visibility = Visibility.Visible;
            });
        }

        /// <summary>
        /// Вызывается, когда клиент игры была закрыта.
        /// </summary>
        private void OnGameClosed()
        {
            Runtime.DebugWrite("Game Exit");
        }


        ///
        /// <!-- Properties Handlers --> ///
        /// 


        /// <summary>
        /// Вызывается при обновлении значения в IsProcessActive.
        /// Изменяет видимость блока с тэгами и progressbar в зависимости от значения IsProcessActive.
        /// </summary>
        private void OnProcessActive()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (IsProcessActive)
                {
                    _tagsPanel.Visibility = Visibility.Collapsed;
                    _progressBar.Visibility = Visibility.Visible;
                }
                else
                {
                    _tagsPanel.Visibility = Visibility.Visible;
                    _progressBar.Visibility = Visibility.Collapsed;
                }
            });
        }

        /// <summary>
        /// Вызывается при обновлении значения в IsDownloading.
        /// Изменяет видимость files count label в зависимости от значения IsDownloading.
        /// </summary>
        private void OnDownloadingChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (IsDownloading)
                {
                    _filesCountLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    _filesCountLabel.Visibility = Visibility.Collapsed;
                }
            });
        }


        #endregion Private Methods


        #region Animations


        /// <summary>
        /// Анимация удаления.
        /// </summary>
        private void DeleteAnimation()
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


        #endregion Animations
    }
}
