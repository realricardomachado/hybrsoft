using Hybrsoft.DTOs;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	public class SettingsService(ISettingsStore settingsStore) : ISettingsService
	{
		private readonly ISettingsStore _settingsStore = settingsStore;

		public string AppName => AppSettings.Current.AppName;
		public string Version => AppSettings.Current.Version;

		public string UserName
		{
			get => AppSettings.Current.UserName;
			set => AppSettings.Current.UserName = value;
		}

		public string UserFirstName
		{
			get => _settingsStore.Read<string>(nameof(UserFirstName));
			set => _settingsStore.Write(nameof(UserFirstName), value);
		}

		public string UserLastName
		{
			get => _settingsStore.Read<string>(nameof(UserLastName));
			set => _settingsStore.Write(nameof(UserLastName), value);
		}

		public char PasswordChar
		{
			get => AppSettings.Current.PasswordChar;
		}

		public EnvironmentType Environment
		{
			get => _settingsStore.ReadEnum(nameof(Environment), EnvironmentType.Development);
			set => _settingsStore.WriteEnum(nameof(Environment), value);
		}

		public bool UseSemanticSearch
		{
			get => AppSettings.Current.UseSemanticSearch;
			set => AppSettings.Current.UseSemanticSearch = value;
		}

		public bool IsSemanticSearchEnabled { get; set; } = true;

		public string LicensedTo
		{
			get
			{
				var license = _settingsStore.ReadSecure<SubscriptionInfoDto>("LicenseData");
				return license?.LicensedTo ?? "Security Administration";
			}
		}
	}
}
