using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.UI.WPF.Core;
using Lexplosion.UI.WPF.Mvvm.Models.Mvvm.InstanceModel;
using Lexplosion.UI.WPF.Services.Descriptions;
using System;
using System.Threading.Tasks;

namespace Lexplosion.UI.WPF.Mvvm.ViewModels.MainContent.InstanceProfile
{
	// No MarkdownWPF or WebView2 dependencies. Data and loading state only.
	public sealed class InstanceProfileOverviewModel : ViewModelBase
	{
		private readonly Action<bool> _changeLoadingStatus;
		private readonly Lazy<Task<InstanceData>> _fullDataTask;
		private InstanceData _fullData;
		private string _overviewDescription = string.Empty;
		private bool _isDescriptionReady;

		public InstanceModelBase InstanceModel { get; }
		// _fullData is the full project response, not the summary/PageData.
		public InstanceData InstanceData => _fullData ?? InstanceModel.PageData;
		public BaseInstanceData BaseInstanceData => InstanceModel.BaseData;
		public bool IsLocal { get; }
		public InstanceSource DescriptionSource => InstanceModel.Source;
		public InstanceData AdditionalData => _fullData;
		public string OverviewDescription => _overviewDescription;
		public bool IsDescriptionReady => _isDescriptionReady;

		public InstanceProfileOverviewModel(InstanceModelBase instanceModel, Action<bool> changeLoadingStatus)
		{
			InstanceModel = instanceModel ?? throw new ArgumentNullException(nameof(instanceModel));
			_changeLoadingStatus = changeLoadingStatus;
			IsLocal = instanceModel.IsLocal || instanceModel.Source == InstanceSource.Nightworld;
			_fullDataTask = new Lazy<Task<InstanceData>>(() => Task.Run(() => InstanceModel.AdditionalData));

			OverviewDiagnostics.Info("Model created; source=" + DescriptionSource +
				"; local=" + IsLocal + "; pageData=" + (instanceModel.PageData != null) +
				"; baseDescriptionLength=" + (instanceModel.BaseData?.Description?.Length ?? 0));

			instanceModel.DataChanged += OnDataChanged;
			if (IsLocal)
			{
				SetDescription(SelectDescription(null), "local");
				_changeLoadingStatus?.Invoke(false);
			}
			else
			{
				_ = LoadDescriptionAsync();
			}
		}

		private async Task LoadDescriptionAsync()
		{
			OverviewDiagnostics.Info("Full data request started via InstanceModel.AdditionalData");
			try
			{
				_fullData = await _fullDataTask.Value;
				OverviewDiagnostics.Info("_fullData fetched via InstanceModel.AdditionalData: null=" +
					(_fullData == null) + "; Description length=" +
					(_fullData?.Description?.Length ?? 0) +
					"; PageData.Description length=" +
					(InstanceModel.PageData?.Description?.Length ?? 0));
				OnPropertyChanged(nameof(AdditionalData));
				OnPropertyChanged(nameof(InstanceData));
				SetDescription(SelectDescription(_fullData), "full-data/fallback");
			}
			catch (Exception ex)
			{
				OverviewDiagnostics.Error("Full data request", ex);
				SetDescription(SelectDescription(null), "error-fallback");
			}
			finally
			{
				_changeLoadingStatus?.Invoke(false);
				OverviewDiagnostics.Info("Full data loading indicator finished");
			}
		}

		private string SelectDescription(InstanceData fullData)
		{
			// PRIMARY: the backend's full response. Do not render the short Summary
			// while this response is still loading.
			if (!string.IsNullOrWhiteSpace(_fullData?.Description))
				return _fullData.Description;
			if (!string.IsNullOrWhiteSpace(InstanceModel.PageData?.Description))
				return InstanceModel.PageData.Description;
			if (!string.IsNullOrWhiteSpace(BaseInstanceData?.Description))
				return BaseInstanceData.Description;
			return BaseInstanceData?.Summary ?? string.Empty;
		}

		private void SetDescription(string value, string reason)
		{
			_overviewDescription = value ?? string.Empty;
			_isDescriptionReady = true;
			OverviewDiagnostics.Info("Description ready; origin=" + reason +
				"; length=" + _overviewDescription.Length);
			OnPropertyChanged(nameof(OverviewDescription));
			OnPropertyChanged(nameof(IsDescriptionReady));
		}

		private void OnDataChanged()
		{
			OnPropertyChanged(nameof(BaseInstanceData));
			OverviewDiagnostics.Info("Instance DataChanged; local=" + IsLocal +
				"; pageDescriptionLength=" + (InstanceModel.PageData?.Description?.Length ?? 0));
			if (IsLocal)
				SetDescription(SelectDescription(null), "local-edited");
			// In case details arrived later through PageData, upgrade a summary-only fallback.
			else if (_isDescriptionReady && _fullData?.Description == null &&
					 !string.IsNullOrWhiteSpace(InstanceModel.PageData?.Description))
				SetDescription(SelectDescription(null), "late-page-data");
		}
	}

	public sealed class InstanceProfileOverviewViewModel : ViewModelBase
	{
		public InstanceProfileOverviewModel Model { get; }

		public InstanceProfileOverviewViewModel(InstanceModelBase instanceModel, Action<bool> changeLoadingStatus)
		{
			Model = new InstanceProfileOverviewModel(instanceModel, changeLoadingStatus);
		}
	}
}
