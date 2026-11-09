using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.DataServices;
using Hybrsoft.UI.Windows.Infrastructure.Common;
using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Hybrsoft.UI.Windows.Services;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Hybrsoft.UI.Windows.ViewModels
{
	public partial class ConnectionSetupViewModel : ViewModelBase
	{
		private readonly IConnectionConfigurationService _connectionConfigurationService;
		private readonly ISettingsService _settingsService;
		private readonly RelayCommand _testConnectionCommand;
		private string _lastTestedConnectionString;

		public ConnectionSetupViewModel(
			IConnectionConfigurationService connectionConfigurationService,
			ISettingsService settingsService,
			ICommonServices commonServices) : base(commonServices)
		{
			_connectionConfigurationService = connectionConfigurationService;
			_settingsService = settingsService;
			_testConnectionCommand = new RelayCommand(
				async () => await TestConnectionAsync(),
				() => !IsBusy);
			LoadCurrentConnectionString();
		}

		public ICommand TestConnectionCommand => _testConnectionCommand;

		private string _server = string.Empty;
		public string Server
		{
			get => _server;
			set => Set(ref _server, value);
		}

		private string _database = string.Empty;
		public string Database
		{
			get => _database;
			set => Set(ref _database, value);
		}

		private int _authenticationIndex;
		public int AuthenticationIndex
		{
			get => _authenticationIndex;
			set
			{
				if (Set(ref _authenticationIndex, value))
				{
					NotifyPropertyChanged(nameof(IsSqlAuthentication));
				}
			}
		}

		public bool IsSqlAuthentication => AuthenticationIndex == 0;

		private string _userName = string.Empty;
		public string UserName
		{
			get => _userName;
			set => Set(ref _userName, value);
		}

		private string _password = string.Empty;
		public string Password
		{
			get => _password;
			set => Set(ref _password, value);
		}

		private bool _trustServerCertificate;
		public bool TrustServerCertificate
		{
			get => _trustServerCertificate;
			set => Set(ref _trustServerCertificate, value);
		}

		private bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
			private set
			{
				if (Set(ref _isBusy, value))
				{
					_testConnectionCommand.OnCanExecuteChanged();
				}
			}
		}

		private string _resultMessage = string.Empty;
		public string ResultMessage
		{
			get => _resultMessage;
			private set => Set(ref _resultMessage, value);
		}

		private InfoBarSeverity _resultSeverity;
		public InfoBarSeverity ResultSeverity
		{
			get => _resultSeverity;
			private set => Set(ref _resultSeverity, value);
		}

		private bool _isResultOpen;
		public bool IsResultOpen
		{
			get => _isResultOpen;
			private set => Set(ref _isResultOpen, value);
		}

		public void LoadCurrentConnectionString()
		{
			if (!_connectionConfigurationService.HasStoredConnectionString)
			{
				return;
			}

			try
			{
				var connectionString = new SqlConnectionStringBuilder(_connectionConfigurationService.CurrentConnectionString);
				Server = connectionString.DataSource;
				Database = connectionString.InitialCatalog;
				AuthenticationIndex = connectionString.IntegratedSecurity ? 1 : 0;
				UserName = connectionString.UserID;
				Password = connectionString.Password;
				TrustServerCertificate = connectionString.TrustServerCertificate;
			}
			catch (ArgumentException ex)
			{
				ShowFailure(DatabaseConnectionFailureReason.InvalidConnectionString, ex.Message);
			}
		}

		public async Task<bool> TestConnectionAsync()
		{
			var connectionString = BuildConnectionString();
			if (connectionString is null)
			{
				return false;
			}

			IsBusy = true;
			try
			{
				var result = await _connectionConfigurationService.TestAsync(connectionString);
				if (result.Succeeded)
				{
					_lastTestedConnectionString = connectionString;
					ResultSeverity = InfoBarSeverity.Success;
					ResultMessage = ResourceService.GetString<ConnectionSetupViewModel>(ResourceFiles.InfoMessages, "ConnectionEstablishedSuccessfully");
					IsResultOpen = true;
					return true;
				}

				ShowFailure(result.FailureReason, result.TechnicalMessage);
				return false;
			}
			finally
			{
				IsBusy = false;
			}
		}

		public async Task<bool> SaveAsync()
		{
			var connectionString = BuildConnectionString();
			if (connectionString is null)
			{
				return false;
			}

			if (!string.Equals(connectionString, _lastTestedConnectionString, StringComparison.Ordinal) &&
				!await TestConnectionAsync())
			{
				return false;
			}

			_connectionConfigurationService.Save(connectionString);
			return true;
		}

		private string BuildConnectionString()
		{
			bool usesWindowsAuthentication = AuthenticationIndex == 1;
			if (string.IsNullOrWhiteSpace(Server) ||
				string.IsNullOrWhiteSpace(Database) ||
				(!usesWindowsAuthentication &&
					(string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))))
			{
				ResultSeverity = InfoBarSeverity.Warning;
				ResultMessage = ResourceService.GetString<ConnectionSetupViewModel>(ResourceFiles.InfoMessages,
					usesWindowsAuthentication
						? "MissingWindowsFields"
						: "MissingSqlFields");
				IsResultOpen = true;
				return null;
			}

			return SQLServerConnectionValidator.CreateConnectionString(new(
				Server.Trim(),
				Database.Trim(),
				usesWindowsAuthentication,
				UserName.Trim(),
				Password,
				TrustServerCertificate,
				_settingsService.AppName));
		}

		private void ShowFailure(DatabaseConnectionFailureReason failureReason, string technicalMessage)
		{
			string resourceKey = failureReason switch
			{
				DatabaseConnectionFailureReason.MissingConnectionString => "EnterTheDatabaseConnectionInformation",
				DatabaseConnectionFailureReason.InvalidConnectionString => "TheDatabaseConnectionInformationIsInvalid",
				_ => "UnableToConnectToTheDatabase"
			};

			string message = ResourceService.GetString<ConnectionSetupViewModel>(ResourceFiles.Errors, resourceKey);
			ResultSeverity = InfoBarSeverity.Error;
			ResultMessage = string.IsNullOrWhiteSpace(technicalMessage)
				? message
				: $"{message}{Environment.NewLine}{technicalMessage}";
			IsResultOpen = true;
		}
	}
}
