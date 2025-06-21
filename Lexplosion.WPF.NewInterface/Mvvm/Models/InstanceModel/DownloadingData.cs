using Lexplosion.WPF.NewInterface.Core.ViewModel;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel
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
                OnPropertyChanged(nameof(StageFormatted));
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
            }
        }
        /// <summary>
        /// Процент скачивания
        /// </summary>
        private int _persentages;
        public int Percentages
        {
            get => _persentages; set
            {
                _persentages = value;
                OnPropertyChanged();
            }
        }


        public string StageFormatted { get; }

        public DownloadingData()
        {
            StageFormatted = $"{FilesCounts}/{TotalFiles}";
            OnPropertyChanged(string.Empty);
        }
    }
}