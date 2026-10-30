using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.DataServices;
using Hybrsoft.UI.Windows.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Views.ConnectionSetup
{
	public sealed partial class ConnectionSetupView : UserControl
	{
		private readonly IConnectionConfigurationService _connectionConfigurationService;
		private readonly IResourceService _resourceService;
		private string _lastTestedConnectionString;

		public ConnectionSetupView()
		{
			this.InitializeComponent();
			_connectionConfigurationService = ServiceLocator.Current.GetService<IConnectionConfigurationService>();
			_resourceService = ServiceLocator.Current.GetService<IResourceService>();
		}

		public async Task<bool> SaveAsync()
		{
			var connectionString = BuildConnectionString();
			if (connectionString is null)
			{
				return false;
			}

			if (!string.Equals(connectionString, _lastTestedConnectionString, StringComparison.Ordinal))
			{
				var result = await TestAsync(connectionString);
				if (!result.Succeeded)
				{
					return false;
				}
			}

			_connectionConfigurationService.Save(connectionString);
			return true;
		}

		private async void TestButton_Click(object sender, RoutedEventArgs e)
		{
			var connectionString = BuildConnectionString();
			if (connectionString is not null)
			{
				await TestAsync(connectionString);
			}
		}

		private void AuthenticationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SqlCredentialsPanel is null)
			{
				return;
			}

			SqlCredentialsPanel.Visibility = AuthenticationComboBox.SelectedIndex == 0
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		private async Task<DatabaseConnectionTestResult> TestAsync(string connectionString)
		{
			TestButton.IsEnabled = false;
			try
			{
				var result = await _connectionConfigurationService.TestAsync(connectionString);
				ResultInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Error;
				ResultInfoBar.Message = result.Succeeded
					? _resourceService.GetString(ResourceFiles.UI, "ConnectionSetupView_ConnectionSucceeded")
					: GetFailureMessage(result);
				ResultInfoBar.IsOpen = true;
				if (result.Succeeded)
				{
					_lastTestedConnectionString = connectionString;
				}
				return result;
			}
			finally
			{
				TestButton.IsEnabled = true;
			}
		}

		private string BuildConnectionString()
		{
			bool usesWindowsAuthentication = AuthenticationComboBox.SelectedIndex == 1;
			if (string.IsNullOrWhiteSpace(ServerTextBox.Text) ||
				string.IsNullOrWhiteSpace(DatabaseTextBox.Text) ||
				(!usesWindowsAuthentication &&
					(string.IsNullOrWhiteSpace(UserNameTextBox.Text) ||
					string.IsNullOrWhiteSpace(PasswordBox.Password))))
			{
				ResultInfoBar.Severity = InfoBarSeverity.Warning;
				ResultInfoBar.Message = usesWindowsAuthentication
					? _resourceService.GetString(ResourceFiles.UI, "ConnectionSetupView_MissingWindowsFields")
					: _resourceService.GetString(ResourceFiles.UI, "ConnectionSetupView_MissingSqlFields");
				ResultInfoBar.IsOpen = true;
				return null;
			}

			return SQLServerConnectionValidator.CreateConnectionString(new(
				ServerTextBox.Text.Trim(),
				DatabaseTextBox.Text.Trim(),
				usesWindowsAuthentication,
				UserNameTextBox.Text.Trim(),
				PasswordBox.Password,
				TrustServerCertificateToggle.IsOn,
				AppSettings.Current.AppName));
		}

		private string GetFailureMessage(DatabaseConnectionTestResult result)
		{
			string resourceKey = result.FailureReason switch
			{
				DatabaseConnectionFailureReason.MissingConnectionString => "DatabaseConnection_MissingConnectionString",
				DatabaseConnectionFailureReason.InvalidConnectionString => "DatabaseConnection_InvalidConnectionString",
				_ => "DatabaseConnection_ConnectionFailed"
			};

			string message = _resourceService.GetString(ResourceFiles.Errors, resourceKey);
			return string.IsNullOrWhiteSpace(result.TechnicalMessage)
				? message
				: $"{message}{Environment.NewLine}{result.TechnicalMessage}";
		}
	}
}
