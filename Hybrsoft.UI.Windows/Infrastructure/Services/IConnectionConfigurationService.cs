using System.Threading;
using System.Threading.Tasks;
using Hybrsoft.Infrastructure.DataServices;

namespace Hybrsoft.UI.Windows.Services
{
	public interface IConnectionConfigurationService
	{
		bool HasStoredConnectionString { get; }
		string CurrentConnectionString { get; }
		Task<DatabaseConnectionTestResult> TestAsync(string connectionString, CancellationToken cancellationToken = default);
		Task<DatabaseConnectionTestResult> TestCurrentAsync(CancellationToken cancellationToken = default);
		void Save(string connectionString);
	}
}
