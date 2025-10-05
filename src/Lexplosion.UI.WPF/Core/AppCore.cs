
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Controls.Message.Core;
using Lexplosion.UI.WPF.Core.Notifications;
using Lexplosion.UI.WPF.Core.Services;
using Lexplosion.UI.WPF.Core.ViewModel;
using Lexplosion.UI.WPF.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Core
{
    public class Gallery : ObservableObject
    {
        public event Action StateChanged;


        private int _imageSourceIndex;

        /// <summary>
        /// ImageSources требуется для возможности листать избражения.
        /// Image Sources не будет иметь возможно изменяться из вне в обход метода ChangeContext.
        /// </summary>
        private List<object> _imageSources = [];
        public IReadOnlyCollection<object> ImageSources { get => _imageSources; }

        public object SelectedImageSource { get; private set; }
        /// <summary>
        /// Наличие следующего изображения.
        /// </summary>
        public bool HasNext { get => _imageSourceIndex < _imageSources.Count - 1; }
        /// <summary>
        /// Наличие предыдущего изображения.
        /// </summary>
        public bool HasPrev { get => _imageSourceIndex > 0; }
        /// <summary>
        /// Наличие выбранного изображения
        /// </summary>
        public bool HasSelectedImage { get => SelectedImageSource != null; }


        public Gallery()
        {

        }

        /// <summary>
        /// Закрывает изображение и очищает ImageSources
        /// </summary>
        public void CloseImage()
        {
            _imageSources.Clear();
            SelectedImageSource = null;
            UpdateState();
        }

        /// <summary>
        /// Заменяет контекст
        /// </summary>
        public void ChangeContext(IEnumerable<object> imageSources)
        {
            _imageSources.Clear();
            _imageSources = new(imageSources);
            OnPropertyChanged(null);
        }

        /// <summary>
        /// Пытается найти изображение в ресурсах заданных при контексте. Сохраняет индекс.
        /// Если изображение не найдено в ресурсах, отрисовывает изображение.
        /// </summary>
        public void SelectImage(object imageSource)
        {
            _imageSourceIndex = -1;

            if (imageSource is string)
            {
                _imageSourceIndex = _imageSources.FindIndex(i => i == imageSource);
            }
            else
            {
                if (imageSource is IEnumerable<byte> bytes)
                {
                    _imageSourceIndex = _imageSources.FindIndex(i => i is IEnumerable<byte> && (i as IEnumerable<byte>).SequenceEqual(bytes));
                }
            }


            SelectedImageSource = imageSource;

            UpdateState();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Prev()
        {
            throw new NotImplementedException();
        }

        private void UpdateState()
        {
            OnPropertyChanged(null);
            StateChanged?.Invoke();
        }
    }

    public sealed class AppResources : INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly IDictionary _resources;

        public AppResources(IDictionary resources)
        {
            _resources = resources;
        }

        public object this[object key]
        {
            get => _resources[key];
            set
            {
                _resources[key] = value;
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace)
                    );
            }
        }

    }

    public sealed class AppCore
    {
        public event Action<GlobalLoadingArgs> GlobalLoadingStarted;


        private readonly Action _restartApp;


        /// <summary>
        /// Метод для выполнения кода в потоке приложения.
        /// Требуется для возможности работать с разными MVVM фремворками
        /// без четкой зависимости на них.
        /// </summary>
        public readonly Action<Action> UIThread;
        /// <summary>
        /// Обертка на ResourceDictionary из WPF, позволяющая работать с ресурсами приложения.
        /// </summary>
        public AppResources Resources { get; }


        #region Properties


        /// <summary>
        /// Настройки приложения
        /// </summary>
        public AppSettings Settings { get; set; }
        /// <summary>
        /// Диалог сервис
        /// </summary>
        /// <summary>
        /// Навигация модалок
        /// </summary>
        public ModalNavigationStore ModalNavigationStore { get; } = new();

        public INavigationStore NavigationStore { get; } = new NavigationStore();

        public IMessageService MessageService { get; }

        public INotificationService NotificationService { get; }


        public Gallery GalleryManager { get; } = new();


        #endregion Properties


        public AppCore(Action<Action> uiThread, AppResources appResources, Action restartApp)
        {
            _restartApp = restartApp;
            Resources = appResources;
            UIThread = uiThread;
            MessageService = new MessageService();
            NotificationService = new NotificationService();
        }


        public ICommand BuildNavigationCommand(ViewModelBase viewModel, Action<ViewModelBase> action = null)
        {
            return BuildNavigationCommand<ViewModelBase>(viewModel, action);
        }

        public ICommand BuildNavigationCommand<T>(T viewModel, Action<T> action = null) where T : ViewModelBase
        {
            return new NavigateCommand<ViewModelBase>(NavigationStore, () =>
            {
                action?.Invoke(viewModel);
                return viewModel;
            });
        }

        public void SetGlobalLoadingStatus(bool status, string processDescription = "", bool isProcessDescriptionKey = false)
        {
            var description = processDescription;

            if (isProcessDescriptionKey)
            {
                description = Resources["processDescription"] as string;
            }

            GlobalLoadingStarted?.Invoke(new GlobalLoadingArgs(status, description));
        }

        public void RestartApp()
        {
            _restartApp();
        }
    }
}
