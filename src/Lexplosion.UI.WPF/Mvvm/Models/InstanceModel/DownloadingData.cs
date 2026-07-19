using Lexplosion.UI.WPF.Core.ViewModel;

namespace Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel
{
    public sealed class DownloadingData : ObservableObject
    {
        /// <summary>
        /// Текущий этап
        /// </summary>
        private StateType _stage;
        public StateType Stage
        {
            get => _stage; set
            {
                _stage = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Всего этапов
        /// </summary>
        private int _totalStages;
        public int TotalStages
        {
            get => _totalStages; set
            {
                _totalStages = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Активный этап
        /// </summary>
        private int _currentStage;
        public int CurrentStage
        {
            get => _currentStage; set
            {
                _currentStage = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Всего файлов
        /// </summary>
        private int _totalFiles;
        public int TotalFiles
        {
            get => _totalFiles; set
            {
                _totalFiles = value;
                OnPropertyChanged();
				OnPropertyChanged(nameof(StageFormatted));
			}
        }
        /// <summary>
        /// Текущие количество скаченных файлов
        /// </summary>
        private int _filesCounts;
        public int FilesCounts
        {
            get => _filesCounts; set
            {
                _filesCounts = value;
                OnPropertyChanged();
				OnPropertyChanged(nameof(StageFormatted));
			}
        }
        /// <summary>
        /// Процент скачивания
        /// </summary>
        private int _percentages;
        public int Percentages
        {
            get => _percentages; set
            {
				_percentages = value;
                OnPropertyChanged();
            }
        }


		public string StageFormatted => $"{FilesCounts}/{TotalFiles}";


		public DownloadingData()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}