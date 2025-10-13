using Lexplosion.Logic.Management.Notifications;
using Lexplosion.UI.WPF.Commands;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.ViewModel;
using System.Linq;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels
{
    public class LatestNewsModel : ObservableObject
    {
        public NotificationsManager NotificationsManager { get; } = Runtime.ServicesContainer.NotificationsService;

        public News LatestUnseenNews { get; }

        public bool HasUnseenNews { get => LatestUnseenNews != null && !LatestUnseenNews.IsViewed; }

        public LatestNewsModel()
        {
            LatestUnseenNews = NotificationsManager.GetUnseenNews().FirstOrDefault();
        }

        public void MarkAsViewed()
        {
            LatestUnseenNews.MarkAsViewed();
            OnPropertyChanged(nameof(HasUnseenNews));
        }
    }

    public sealed class LatestNewsViewModel : ViewModelBase
    {
        public LatestNewsModel Model { get; }

        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get => RelayCommand.GetCommand(ref _closeCommand, () =>
            {
                Model.MarkAsViewed();
            });
        }

        public LatestNewsViewModel()
        {
            Model = new();
        }
    }
}
