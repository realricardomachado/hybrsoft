using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Common
{
	public interface IStartupView
	{
		Task InitializationTask { get; }
	}
}
