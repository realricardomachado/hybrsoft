using CommunityToolkit.WinUI.Helpers;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Microsoft.Extensions.Configuration;
using System;
using Windows.ApplicationModel;
using Windows.Graphics;
using Windows.Storage;

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

		private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

		public string AppName => AppInfo.Current.DisplayInfo.DisplayName;
		public string Version => Package.Current.Id.Version.ToFormattedString();

		public SizeInt32 WindowSizeDefault
		{
			get => new() { Width = WindowWidth, Height = WindowHeight };
			set { WindowWidth = value.Width; WindowHeight = value.Height; }
		}
		private int WindowWidth
		{
			get => ReadBootstrapSetting(nameof(WindowWidth), 1280);
			set => LocalSettings.Values[nameof(WindowWidth)] = value;
		}
		private int WindowHeight
		{
			get => ReadBootstrapSetting(nameof(WindowHeight), 840);
			set => LocalSettings.Values[nameof(WindowHeight)] = value;
		}

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

		private static T ReadBootstrapSetting<T>(string key, T defaultValue = default)
		{
			try
			{
				if (!LocalSettings.Values.TryGetValue(key, out var value))
					return defaultValue;

				if (value is T typed)
					return typed;

				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error reading bootstrap setting '{key}': {ex.Message}");
				return defaultValue;
			}
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
