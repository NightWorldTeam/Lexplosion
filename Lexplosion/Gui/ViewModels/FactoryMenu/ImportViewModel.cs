using Lexplosion.Gui.Models;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public class ImportViewModel : VMBase
    {
        private MainViewModel _mainViewModel;

        public RelayCommand ImportCommand 
        {
            get => new RelayCommand(obj => 
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();

                // Process open file dialog box results
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    InstanceClient? instanceClient;
                    ImportResult result = InstanceClient.Import(dialog.FileName, out instanceClient);

                    if (instanceClient == null || result != ImportResult.Successful) 
                    {
                        MainViewModel.ShowToastMessage("Импорт завершился с ошибкой", result.ToString(), Controls.ToastMessageState.Error);
                        return;
                    }

                    MainViewModel.ShowToastMessage("Результат импорта", "Импорт завершился успешно, хотите запустить сборку?", Controls.ToastMessageState.Notification);
                }
            }); 
        }

        public ImportViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
    }
}
