using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Hybrsoft.UI.Windows.Models;
using Hybrsoft.UI.Windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.UI.Windows.ViewModels
{
	public partial class SettingsViewModel(ISettingsService settingsService,
		IEnumerable<IEmbeddingTables> embeddingTables,
		ICommonServices commonServices) : ViewModelBase(commonServices)
	{
		private readonly ISettingsService _settingsService = settingsService;
		private readonly IEnumerable<IEmbeddingTables> _embeddingTables = embeddingTables;

		public string AppName => $"{_settingsService.AppName}";
		public string Version => $"{_settingsService.Version}";
		public string LicenseTo;

		private bool _isBusy = false;
		public bool IsBusy
		{
			get => _isBusy;
			set => Set(ref _isBusy, value);
		}

		private List<LanguageModel> _availableLanguages;
		public List<LanguageModel> AvailableLanguages
		{
			get { return _availableLanguages; }
			set { Set(ref _availableLanguages, value); }
		}

		private LanguageModel _selectedLanguage;
		public LanguageModel SelectedLanguage
		{
			get { return _selectedLanguage; }
			set
			{
				if (Set(ref _selectedLanguage, value) is true)
				{
					OnSelectedLanguageChanged(value);
				}
			}
		}
		private void OnSelectedLanguageChanged(LanguageModel value)
		{
			string title = ResourceService.GetString<SettingsViewModel>(ResourceFiles.Warnings, "TheLanguageHasBeenSuccessfullyChanged");
			string message = ResourceService.GetString<SettingsViewModel>(ResourceFiles.Warnings, "PleaseRestartTheAppForTheChangesToBeFullyApplied");
			WarningMessage(title, message);
			LogSuccess("Settings", "LanguageChanged", "Language changed successfully",$"The language has been successfully changed to {value.DisplayName} ({value.Tag}).");

			_ = ResourceService.SetLanguageAsync(value);
		}

		private bool _useSemanticSearch = false;
		public bool UseSemanticSearch
		{
			get => _useSemanticSearch;
			set
			{
				if (Set(ref _useSemanticSearch, value) is true)
				{
					_settingsService.UseSemanticSearch = value;
					if (value)
					{
						GenerateMissingEmbeddings();
					}
				}
			}
		}
		private bool _isSemanticSearchEnabled;
		public bool IsSemanticSearchEnabled
		{
			get => _isSemanticSearchEnabled;
			set => Set(ref _isSemanticSearchEnabled, value);
		}

		public SettingsArgs ViewModelArgs { get; private set; }

		public async Task LoadAsync(SettingsArgs args)
		{
			ViewModelArgs = args ?? SettingsArgs.CreateDefault();
			_availableLanguages = ResourceService.LanguageOptions;
			_selectedLanguage = ResourceService.GetCurrentLanguageItem();
			_useSemanticSearch = _settingsService.UseSemanticSearch;
			_isSemanticSearchEnabled = _settingsService.IsSemanticSearchEnabled;

			string licenseToPrefix = ResourceService.GetString<SettingsViewModel>(ResourceFiles.UI, "LicensedTo_Prefix");
			LicenseTo = licenseToPrefix + _settingsService.LicensedTo;

			StatusReady();
		}

		private void GenerateMissingEmbeddings()
		{
			string startTitle = ResourceService.GetString(ResourceFiles.InfoMessages, "Processing");
			string startMessage = ResourceService.GetString<SettingsViewModel>(ResourceFiles.InfoMessages, "PreparingTheSystemForSmarterSearches");
			StartStatusMessage(startTitle, startMessage);
			IsBusy = true;
			_ = ContextService.RunAsync(async () =>
			{
				try
				{
					var tasks = _embeddingTables.Select(table => table.PopulateMissingEmbeddingsAsync());
					await Task.WhenAll(tasks);
					string endTitle = ResourceService.GetString(ResourceFiles.InfoMessages, "SaveSuccessful");
					string endMessage = ResourceService.GetString<SettingsViewModel>(ResourceFiles.InfoMessages, "SetupCompletedSmartSearchIsNowAvailable");
					EndStatusMessage(endTitle, endMessage, LogType.Success);
				}
				catch (Exception ex)
				{
					string title = ResourceService.GetString(ResourceFiles.Errors, "ProcessFailed");
					string message = ResourceService.GetString<SettingsViewModel>(ResourceFiles.Errors, "CouldNotCompleteTheSmartSearchSetupPleaseTryAgainLater");
					StatusError(title, message);
					LogException("Settings", "GenerateEmbeddingsFailed", ex);
				}
				finally
				{
					IsBusy = false;
				}
			});
		}
	}
}
