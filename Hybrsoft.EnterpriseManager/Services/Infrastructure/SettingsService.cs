using Hybrsoft.DTOs;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Windows.Graphics;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	public class SettingsService(ISettingsStore settingsStore) : ISettingsService
	{
		private const string WindowWidthKey = "WindowWidth";
		private const string WindowHeightKey = "WindowHeight";
		private readonly ISettingsStore _settingsStore = settingsStore;

		public string AppName => AppSettings.Current.AppName;
		public string Version => AppSettings.Current.Version;

		public string UserName
		{
			get => AppSettings.Current.UserName;
			set => AppSettings.Current.UserName = value;
		}

		public SizeInt32 WindowSize
		{
			get => new()
			{
				Width = _settingsStore.Read(WindowWidthKey, 1280),
				Height = _settingsStore.Read(WindowHeightKey, 840)
			};
			set
			{
				_settingsStore.Write(WindowWidthKey, value.Width);
				_settingsStore.Write(WindowHeightKey, value.Height);
			}
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
