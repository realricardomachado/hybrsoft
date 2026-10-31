using CommunityToolkit.WinUI.Helpers;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Microsoft.Extensions.Configuration;
using System;
using Windows.ApplicationModel;

namespace Hybrsoft.EnterpriseManager.Configuration
{
	public class AppSettings
	{
		static AppSettings()
		{
			Current = new AppSettings();
		}
		public static AppSettings Current { get; }

		private static ISettingsStore SettingsStore
			=> ServiceLocator.Current.GetService<ISettingsStore>();

		public string AppName => AppInfo.Current.DisplayInfo.DisplayName;
		public string Version => Package.Current.Id.Version.ToFormattedString();

		public long UserID
		{
			get => SettingsStore.Read<long>(nameof(UserID));
			set => SettingsStore.Write(nameof(UserID), value);
		}

		public string UserName
		{
			get => SettingsStore.Read<string>(nameof(UserName));
			set => SettingsStore.Write(nameof(UserName), value);
		}

		public string WindowsHelloPublicKeyHint
		{
			get => SettingsStore.Read<string>(nameof(WindowsHelloPublicKeyHint));
			set => SettingsStore.Write(nameof(WindowsHelloPublicKeyHint), value);
		}

		public DataProviderType DataProvider
		{
			get => SettingsStore.ReadEnum(nameof(DataProvider), DataProviderType.SQLServer);
			set => SettingsStore.WriteEnum(nameof(DataProvider), value);
		}

		public string SQLServerConnectionString
		{
			get => SettingsStore.ReadSecure(nameof(SQLServerConnectionString), GetDefaultSQLServerConnectionString());
			set => SettingsStore.WriteSecure(nameof(SQLServerConnectionString), value);
		}

		public bool HasStoredSQLServerConnectionString => SettingsStore.TryRead(nameof(SQLServerConnectionString));

		public bool IsRandomErrorsEnabled
		{
			get => SettingsStore.Read<bool>(nameof(IsRandomErrorsEnabled));
			set => SettingsStore.Write(nameof(IsRandomErrorsEnabled), value);
		}

		public char PasswordChar
		{
			get => SettingsStore.Read(nameof(PasswordChar), '*');
			set => SettingsStore.Write(nameof(PasswordChar), value);
		}

		public string JwtToken
		{
			get => SettingsStore.ReadSecure<string>(nameof(JwtToken));
			set => SettingsStore.WriteSecure(nameof(JwtToken), value);
		}

		public DateTimeOffset JwtTokenExpiration
		{
			get => SettingsStore.ReadSecure(nameof(JwtTokenExpiration), DateTimeOffset.MinValue);
			set => SettingsStore.WriteSecure(nameof(JwtTokenExpiration), value);
		}

		public bool UseSemanticSearch
		{
			get => SettingsStore.Read<bool>(nameof(UseSemanticSearch));
			set => SettingsStore.Write(nameof(UseSemanticSearch), value);
		}

		private static string GetDefaultSQLServerConnectionString()
		{
#if DEBUG
			return new ConfigurationBuilder()
				.AddUserSecrets<AppSettings>(optional: true)
				.Build()
				.GetConnectionString("SQLServerDev") ?? string.Empty;
#else
			return string.Empty;
#endif
		}
	}
}
