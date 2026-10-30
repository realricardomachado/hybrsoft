using Hybrsoft.Enums;

namespace Hybrsoft.UI.Windows.Services
{
	public interface ISettingsService
	{
		string AppName { get; }
		string Version { get; }
		string UserName { get; set; }
		string UserFirstName { get; set; }
		string UserLastName { get; set; }
		char PasswordChar { get; }
		EnvironmentType Environment { get; set; }
		bool UseSemanticSearch { get; set; }
		bool IsSemanticSearchEnabled { get; set; }
		string LicensedTo { get; }
	}
}
