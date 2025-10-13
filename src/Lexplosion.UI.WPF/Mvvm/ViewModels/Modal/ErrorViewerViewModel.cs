using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Core.Modal;
using Lexplosion.UI.WPF.Core.ViewModel;
using System.Collections.Generic;
using System.Text;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.Modal
{
    public readonly struct InstanceInfo
    {
        public InstanceInfo(string name, string version, string modloader, IEnumerable<string> addons)
        {

        }
    }

    public readonly struct InstanceErrorInfo
    {
        public readonly string Title;
        public readonly string Description;
        public readonly IEnumerable<string> AdditionalInfo;

        public InstanceErrorInfo(string errorTitle, string errorDescription, IEnumerable<string> addtionalInfo)
        {

        }
    }

    public sealed class ErrorViewerModel : ObservableObject
    {
        private string _complileMessage;

        public FiltableObservableCollection AdditionalInfo { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText; set
            {
                _searchText = value;
                OnSearchTextChanged(value);
            }
        }

        public ErrorViewerModel(InstanceErrorInfo errorInfo)
        {
            AdditionalInfo = new();
            AdditionalInfo.Source = errorInfo.AdditionalInfo;

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Заголовок: {errorInfo.Title}");
            stringBuilder.AppendLine($"Описание: {errorInfo.Description}");
            stringBuilder.AppendLine($"Дополнительная информация:");

            foreach (string log in errorInfo.AdditionalInfo)
            {
                stringBuilder.AppendLine(log);
            }

            _complileMessage = stringBuilder.ToString();
        }

        private void OnSearchTextChanged(string value)
        {
            AdditionalInfo.Filter = (item) =>
            {
                return (item as string).IndexOf(SearchText, System.StringComparison.InvariantCultureIgnoreCase) > -1;
            };
        }

        public void SaveToFile()
        {
            //var dialog = new System.Windows.Forms.SaveFileDialog()
            //{
            //    FileName = $"{_gameManager.GameClientName} {_gameManager.GameVersion} {DateTime.Now}".Replace(":", "_"),
            //    Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
            //};

            //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    File.WriteAllText(dialog.FileName, );
            //}
        }
    }

    public sealed class ErrorViewerViewModel : ModalViewModelBase
    {
        public ErrorViewerModel Model { get; }

        public ErrorViewerViewModel(InstanceErrorInfo errorInfo)
        {
            Model = new ErrorViewerModel(errorInfo);
        }
    }
}
