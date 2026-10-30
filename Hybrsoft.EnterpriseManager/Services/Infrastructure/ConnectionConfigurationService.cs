using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Infrastructure.DataServices;
using Hybrsoft.UI.Windows.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	public sealed class ConnectionConfigurationService : IConnectionConfigurationService
	{
		public bool HasStoredConnectionString =>
			AppSettings.Current.HasStoredSQLServerConnectionString;

		public string CurrentConnectionString =>
			AppSettings.Current.SQLServerConnectionString;

		public void Save(string connectionString)
		{
			AppSettings.Current.SQLServerConnectionString = connectionString;
		}

		public Task<DatabaseConnectionTestResult> TestCurrentAsync(
			CancellationToken cancellationToken = default)
		{
			return TestAsync(CurrentConnectionString, cancellationToken);
		}

		public Task<DatabaseConnectionTestResult> TestAsync(
			string connectionString,
			CancellationToken cancellationToken = default)
		{
			return SQLServerConnectionValidator.TestAsync(connectionString, cancellationToken);
		}
	}
}
