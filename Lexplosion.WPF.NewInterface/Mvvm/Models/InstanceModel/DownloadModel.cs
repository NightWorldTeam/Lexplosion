using Lexplosion.Logic.Management;
using Lexplosion.Logic.Management.Instances;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
{
    public sealed class DownloadModel
    {
        private readonly InstanceClient _instanceClient;

        /// <summary>
        /// Информация о прогресс было изменено.
        /// </summary>
        public event Action<StageType, ProgressHandlerArguments> ProgressChanged;
        /// <summary>
        /// Процесс скачивание клиент игры завершено.
        /// </summary>
        public event Action<InstanceInit, IEnumerable<string>, bool> Completed;
        /// <summary>
        /// Процесс скачивания клиента игры запущен.
        /// </summary>
        public event Action Started;
        /// <summary>
        /// Процесс отмены скачивания клиента игры был завершен.
        /// </summary>
        public event Action Canceled;


        private bool _isActive;
        public bool IsActive 
        { 
            get => _isActive; private set 
            {
                _isActive = value;
                //Runtime.DebugWrite("IsDownloadActive: " + _isActive);
            }
        }
        

        #region Constructors


        public DownloadModel(InstanceClient instanceClient)
        {
            _instanceClient = instanceClient;

            // st - stage
            // pha - Progress Handler Arguments
            _instanceClient.ProgressHandler += (st, pha) => ProgressChanged?.Invoke(st, pha);
            _instanceClient.DownloadComplited += OnDownloadCompleted;
            _instanceClient.DownloadCanceled += OnDownloadCanceled; 
        }


        #endregion Constructors


        #region Public Methods


        /// <summary>
        /// Запускает скачивание сборки.
        /// </summary>
        /// <param name="version">Версия сборки сборки (not minecraft)</param>
        public void Download(string version = null)
        {
            IsActive = true;
            Lexplosion.Runtime.TaskRun(() =>
            {
                _instanceClient.Update(version);
            });
        }

        /// <summary>
        /// Отменяет текущие скачивание
        /// </summary>
        public async void DownloadCancel()
        {
            IsActive = false;
            await Task.Run(_instanceClient.CancelDownload);
        }


        #endregion Public Methods


        #region Private Methods


        private void OnDownloadCompleted(InstanceInit result, IEnumerable<string> downloadErrors, bool IsGameRun)
        {
            Runtime.DebugWrite("DonwloadModel - Download Complated");
            IsActive = false;
            
            Completed?.Invoke(result, downloadErrors, IsGameRun);

            switch (result)
            {
                case InstanceInit.Successful:
                    {
                    }
                    break;
                case InstanceInit.DownloadFilesError:
                    {
                    }
                    break;
                case InstanceInit.CurseforgeIdError:
                    {
                    }
                    break;
                case InstanceInit.NightworldIdError:
                    {
                    }
                    break;
                case InstanceInit.ServerError:
                    {
                    }
                    break;
                case InstanceInit.GuardError:
                    {
                    }
                    break;
                case InstanceInit.VersionError:
                    {
                    }
                    break;
                case InstanceInit.ForgeVersionError:
                    {
                    }
                    break;
                case InstanceInit.GamePathError:
                    {
                    }
                    break;
                case InstanceInit.ManifestError:
                    {
                    }
                    break;
                case InstanceInit.JavaDownloadError:
                    {
                    }
                    break;
                case InstanceInit.IsCancelled:
                    {
                        break;
                    }
                default:
                    {
                    }
                    break;
            }
        }


        /// <summary>
        /// Вызывается, когда процесс отмены скачивания клиента сборки был завершен.
        /// </summary>
        private void OnDownloadCanceled()
        {
            Canceled?.Invoke();
        }


        #endregion Private Methods
    }
}
