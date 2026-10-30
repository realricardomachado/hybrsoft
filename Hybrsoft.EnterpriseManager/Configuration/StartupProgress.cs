namespace Hybrsoft.EnterpriseManager.Configuration
{
	public sealed record StartupProgress(int Value, string Message, bool IsIndeterminate = false);
}
