using Lexplosion.Common.Models.Objects;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Lexplosion.Common.ViewModels.MainMenu.Multiplayer
{
    public class AdServersListTabViewModel : VMBase
    {
        public ObservableCollection<AdServer> Servers { get; } = new ObservableCollection<AdServer>();


        #region Commands


        private RelayCommand _connectToServerCommand;
        public ICommand ConnectToServerCommand
        {
            get => _connectToServerCommand ?? (_connectToServerCommand = new RelayCommand(obj =>
            {
                var server = obj as AdServer;
                server.ConnectTo();
                // TODO: Тут должно открываться окошко с выбором, клиента!!
            }));
        }


        private RelayCommand _copyAddressCommand;
        public ICommand CopyAddressCommand
        {
            get => _copyAddressCommand ?? (_copyAddressCommand = new RelayCommand(obj =>
            {
                var server = obj as AdServer;
                server.CopyAddressToClipboard();
            }));
        }


        #endregion Commands


        public AdServersListTabViewModel()
        {
            Servers.Add(new AdServer("Legacies", "1.19.2", "0.0.0.0", "3713", "На основе ванильного шейдера игрок BalintCsala создал ресурс-пак, добавляющий в игру эффекты глобального освещения: реалистичные тени от солнца, цветное освещение, отражения и другие\r\n"));

            for (var i = 0; i < 25; i++)
            {
                Servers.Add(new AdServer("Some server " + i, "1.12.2", "192.0.0.0", "1010", "Из-за DMCA Mojang автор мода Haubna был вынужден сделать свой мод полностью бесплатным. Автору так же заблокировали аккаунт на Patreon, из-за чего пропала возможность полный день работать над модом и получать доход. Теперь у Haubna есть аккаунт на Ko-Fi, где можно поддержать автора и скачать Pro версию мода бесплатно."));
            }
        }


        #region Private Methods



        #endregion Private Methods
    }
}
