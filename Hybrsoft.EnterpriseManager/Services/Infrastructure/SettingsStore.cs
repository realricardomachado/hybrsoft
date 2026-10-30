using Hybrsoft.UI.Windows.Services;
using System;
using System.Text.Json;
using Windows.Storage;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	public sealed class SettingsStore(IWindowsSecurityService securityService) : ISettingsStore
	{
		private readonly IWindowsSecurityService _securityService = securityService;

		private static ApplicationDataContainer LocalSettings
			=> ApplicationData.Current.LocalSettings;

		public bool TryRead(string key)
		{
			try
			{
				return LocalSettings.Values.ContainsKey(key);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return false;
			}
		}

		public T Read<T>(string key, T defaultValue = default)
		{
			try
			{
				if (!LocalSettings.Values.TryGetValue(key, out var value))
				{
					return defaultValue;
				}

				if (value is T typedValue)
				{
					return typedValue;
				}

				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch
			{
				return defaultValue;
			}
		}

		public void Write<T>(string key, T value)
		{
			LocalSettings.Values[key] = value;
		}

		public T ReadJson<T>(string key, T defaultValue = default)
		{
			try
			{
				if (!LocalSettings.Values.TryGetValue(key, out var value))
				{
					return defaultValue;
				}

				if (value is not string json)
				{
					return defaultValue;
				}

				return JsonSerializer.Deserialize<T>(json)
					?? defaultValue;
			}
			catch
			{
				return defaultValue;
			}
		}

		public void WriteJson<T>(string key, T value)
		{
			LocalSettings.Values[key] = JsonSerializer.Serialize(value);
		}

		public T ReadSecure<T>(string key, T defaultValue = default)
		{
			try
			{
				var encrypted = ReadJson<string>(key);

				if (string.IsNullOrEmpty(encrypted))
				{
					return defaultValue;
				}

				return _securityService.DecryptData<T>(encrypted);
			}
			catch
			{
				return defaultValue;
			}
		}

		public void WriteSecure<T>(string key, T value)
		{
			var encrypted = _securityService.EncryptData(value);

			WriteJson(key, encrypted);
		}

		public TEnum ReadEnum<TEnum>(string key, TEnum defaultValue) where TEnum : struct, Enum
		{
			var defaultUnderlying = Convert.ChangeType(defaultValue, Enum.GetUnderlyingType(typeof(TEnum)));

			var value = Read(key, defaultUnderlying);

			return (TEnum)Enum.ToObject(typeof(TEnum), value);
		}

		public void WriteEnum<TEnum>(string key, TEnum value) where TEnum : struct, Enum
		{
			var underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(TEnum)));

			Write(key, underlyingValue);
		}
	}
}
