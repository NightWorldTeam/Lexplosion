using Lexplosion.Common.ViewModels.ModalVMs.InstanceTransfer;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Tools;
using System;
using System.Threading.Tasks;

namespace Lexplosion.Common.ViewModels.ModalVMs
{
    public sealed class ExportViewModel : ExportBase
    {
        public ExportViewModel(InstanceClient instanceClient, Action<string, string, uint, byte> doNotification = null) : base(instanceClient, doNotification)
        {

        }

        /// <summary>
        /// Собственно, открытие диалогового окна.
        /// Вызов экспорта.
        /// </summary>
        protected async override void Action()
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = @"C:\Users\GamerStorm_Hel2x_\night-world\export";
                saveFileDialog.Filter = "zip files (*.zip)|*.zip";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = _instanceClient.LocalId + ".zip";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IsExportFinished = false;
                    MainViewModel.ExportedInstance.Add(this);

                    var result = await Task.Run(() => _instanceClient.Export(UnitsList, saveFileDialog.FileName, InstanceName));

                    MainViewModel.ExportedInstance.Remove(this);

                    if (result == ExportResult.Successful)
                    {
                        DoNotification(
                            ResourceGetter.GetString("instanceExport"),
                            String.Format(ResourceGetter.GetString("instanceExportSuccessfulOpenFolder"), InstanceName), 0, 0);

                        IsExportFinished = true;
                        ModalWindowViewModelSingleton.Instance.Close();
                    }
                    else
                    {
                        DoNotification(
                            result.ToString(),
                            String.Format(ResourceGetter.GetString("instanceExportUnsuccessful"), InstanceName), 0, 1);

                        IsExportFinished = true;
                    }
                }
            }
        }
    }
}
