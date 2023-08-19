using Lexplosion.WPF.NewInterface.Models.InstanceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public partial class InstanceForm
    {
        private const string DEFAULT_FORM_STYLE_KEY = "DefaultInstanceForm";
        private const string LOADING_FORM_STYLE_KEY = "LoadingInstanceForm";
        private const string LAUNCH_FORM_STYLE_KEY = "LaunchInstanceForm";

        private const string PART_LOGOBLOCK = "PART_LogoBlock";

        // Overview Info
        private const string PART_LOGO = "PART_Logo";
        private const string PART_NAME = "PART_Name";
        private const string PART_AUTHOR = "PART_Author";
        private const string PART_SHORT_DESCRIPTION = "PART_Description";
        private const string PART_TAGS_PANEL = "PART_TagsPanel";

        // Buttons
        private const string PART_MAIN_ACTION_BUTTON = "PART_MainActionButton";
        private const string PART_DROPDOWNMENU_CONTENT = "PART_DropdownMenuContent";

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

        public enum InstanceFormState
        {
            Default,
            Loading,
            Launching
        }

        public enum MainActionFunc
        {

        }
    }

    public partial class InstanceForm 
    {
        public readonly struct MainActionButtonModel
        {
            public string IconKey { get; }
            public string TextKey { get; }
            public MainActionFunc Action { get; }
        }


    }

    [TemplatePart(Name = PART_LOGOBLOCK, Type = typeof(Grid))]
    [TemplatePart(Name = PART_LOGO, Type = typeof(Border))]
    [TemplatePart(Name = PART_NAME, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_AUTHOR, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_SHORT_DESCRIPTION, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_TAGS_PANEL, Type = typeof(ItemsControl))]
    [TemplatePart(Name = PART_MAIN_ACTION_BUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_DROPDOWNMENU_CONTENT, Type = typeof(ItemsControl))]
    public sealed partial class InstanceForm : Control
    {
        private Grid _logoGridBlock;
        private Border _logoBorder;
        private TextBlock _nameTextBlock;
        private TextBlock _authorTextBlock;
        private TextBlock _shortDescriptionTextBlock;
        private ItemsControl _tagsPanel;

        private Button _mainActionButton;
        private ItemsControl _dropdownMenuItemsControl;

        /// <summary>
        /// Вызывает когда метод OnApplyTemplate завершает работу.
        /// </summary>
        public event Action ApplyTemplateExecuted;

        // -- State -- //

        public event Action<InstanceFormState> StateChanged;

        private InstanceFormState _state;
        public InstanceFormState State
        {
            get => _state; private set
            {
                _state = value;
                StateChanged?.Invoke(value);
            }
        }

        // -- State -- //



        private ObservableCollection<LowerMenuButton> _lowerMenuButtons = new ObservableCollection<LowerMenuButton>();
        public IEnumerable<LowerMenuButton> LowerMenuButtons { get => _lowerMenuButtons; }
        private readonly Dictionary<InstanceFormState, MainActionButtonModel> _stateByMainAction = new Dictionary<InstanceFormState, MainActionButtonModel>();


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


        #region Constructors


        static InstanceForm()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InstanceForm), new FrameworkPropertyMetadata(typeof(InstanceForm)));
        }

        public InstanceForm()
        {
            Style = (Style)App.Current.Resources["DefaultInstanceForm"];
        }


        #endregion Constructors


        #region Public & Protected Methods


        public override void OnApplyTemplate()
        {
            Runtime.DebugWrite("OnApplyTemplate");

            _logoGridBlock = Template.FindName(PART_LOGOBLOCK, this) as Grid;
            _logoBorder = Template.FindName(PART_LOGO, this) as Border;
            _nameTextBlock = Template.FindName(PART_NAME, this) as TextBlock;
            _authorTextBlock = Template.FindName(PART_AUTHOR, this) as TextBlock;
            _shortDescriptionTextBlock = Template.FindName(PART_SHORT_DESCRIPTION, this) as TextBlock;
            _tagsPanel = Template.FindName(PART_TAGS_PANEL, this) as ItemsControl;
            _dropdownMenuItemsControl = Template.FindName(PART_DROPDOWNMENU_CONTENT, this) as ItemsControl;
            _mainActionButton = Template.FindName(PART_MAIN_ACTION_BUTTON, this) as Button;

            if (_logoGridBlock != null)
            {
                _logoGridBlock.MouseLeftButtonDown += _logoGridBlock_MouseLeftButtonDown;
            }

            if (_mainActionButton != null) 
            {
                _mainActionButton.Click += _mainActionButton_Click;
            }

            if (_dropdownMenuItemsControl != null)
            {
                _dropdownMenuItemsControl.ItemsSource = LowerMenuButtons;

                _lowerMenuButtons.Add(new LowerMenuButton(0, IK_DELETE, "DeleteInstance", () => { }));
                _lowerMenuButtons.Add(new LowerMenuButton(1, IK_EXPORT, "Export", () => { }));
                _lowerMenuButtons.Add(new LowerMenuButton(2, IK_OPENFOLDER, "OpenFolder", () => { }));
                _lowerMenuButtons.Add(new LowerMenuButton(3, IK_DLC, "ExploreAddons", () => { }));
                _lowerMenuButtons.Add(new LowerMenuButton(4, IK_LANG, "VisitNightWorld", () => { }));
                _lowerMenuButtons.Add(new LowerMenuButton(5, IK_REMOVE_FROM_LIBRARY, "RemoveFromLibrary", () => { }));
                _lowerMenuButtons.Add(new LowerMenuButton(5, IK_ADD_TO_LIBRARY, "AddToLibrary", () => { }));
            }

            UpdateAllFields(InstanceModel);

            base.OnApplyTemplate();
            ApplyTemplateExecuted?.Invoke();
        }

        private void _mainActionButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case InstanceFormState.Default:
                    {
                        if (!InstanceModel.IsInstalled)
                        {
                            PlayButtonFunc();
                        }
                        else
                        {
                            DownloadButtonFunc();
                        }
                        break;
                    }
                case InstanceFormState.Loading:
                    {
                        PercentageButtonFunc();
                        break;
                    }
                case InstanceFormState.Launching:
                    {
                        CloseButtonFunc();
                        break;
                    }
                default:
                    break;
            }
        }

        private void PercentageButtonFunc() 
        {
            
        }

        private void CloseButtonFunc() 
        {
            State = InstanceFormState.Default;
            SetDefaultFormStyle();
        }

        private void DownloadButtonFunc()
        {
            if (Style != (Style)App.Current.Resources[LOADING_FORM_STYLE_KEY])
            {
                State = InstanceFormState.Loading;
                SetLoadingFormStyle();
                ApplyTemplateExecuted += () =>
                {
                    SetShortDescription("Подготовка к скачиванию");
                };
            }
            else
            {
                State = InstanceFormState.Default;
                SetDefaultFormStyle();
                ApplyTemplateExecuted += () =>
                {
                    SetShortDescription(InstanceModel.ShortDescription);
                };
            }
        }

        private void PlayButtonFunc() 
        {
            if (Style != (Style)App.Current.Resources[LAUNCH_FORM_STYLE_KEY])
            {
                State = InstanceFormState.Launching;
                SetLaunchFormStyle();
                ApplyTemplateExecuted += () =>
                {
                    SetShortDescription("Проверка целостности игровых файлов.");
                };
            }
            else
            {
                State = InstanceFormState.Default;
                SetDefaultFormStyle();
                ApplyTemplateExecuted += () =>
                {
                    SetShortDescription(InstanceModel.ShortDescription);
                };
            }
        }


        public static BitmapImage ToImage(byte[] imageBytes)
        {
            if (imageBytes is null || imageBytes.Length == 0)
                return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png"));

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


        private void SetName(string value) 
        {
            if (_nameTextBlock == null)
                return;

            _nameTextBlock.Text = value;
        }

        private void SetShortDescription(string value)
        {
            if (_shortDescriptionTextBlock == null)
                return;

            _shortDescriptionTextBlock.Text = value;
        }

        private void SetAuthor(string value)
        {
            if (_authorTextBlock == null)
                return;

            _authorTextBlock.Text = "by " + value;
        }

        private void SetCategories(IEnumerable<object> objects) 
        {
            if (_tagsPanel == null)
                return;
            
            _tagsPanel.ItemsSource = objects;
        }

        private void SetLogo(byte[] logo) 
        {
            if (_logoBorder == null)
                return;

            _logoBorder.Background = new ImageBrush()
            {
                ImageSource = ToImage(logo)
            };
        }

        private void UpdateAllFields(InstanceModelBase model) 
        {
            SetName(model.Name);
            SetShortDescription(model.ShortDescription);
            SetCategories(model.Tags);
            SetAuthor(model.Author);
            SetLogo(model.Logo);
        }

        /// <summary>
        /// Устанавливает отображение загрузочной формы.
        /// </summary>
        private void SetLoadingFormStyle() 
        {
            Style = (Style)App.Current.Resources[LOADING_FORM_STYLE_KEY];
        }

        /// <summary>
        /// Устанавливает отображение стандартной формы.
        /// </summary>
        private void SetDefaultFormStyle() 
        {
            Style = (Style)App.Current.Resources[DEFAULT_FORM_STYLE_KEY];
        }

        /// <summary>
        /// Устанавливает отображение стандартной формы.
        /// </summary>
        private void SetLaunchFormStyle() 
        {
            Style = (Style)App.Current.Resources[LAUNCH_FORM_STYLE_KEY];
        }


        private static void OnInstanceModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instanceForm = d as InstanceForm;
            var instanceModelBase = ((InstanceModelBase)e.NewValue);

            instanceModelBase.NameChanged += instanceForm.SetName;

            instanceForm.UpdateAllFields(instanceModelBase);
        }


        private void ChangeMainActionButtonFunc(InstanceFormState instanceFormState)
        {
            //_stateByMainAction[instanceFormState]
        }


        #region Logo Grid Block


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


        #endregion Logo Grid Block


        #endregion Private Methods
    }
}
