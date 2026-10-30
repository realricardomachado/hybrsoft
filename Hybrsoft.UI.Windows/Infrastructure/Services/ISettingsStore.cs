using System;

namespace Hybrsoft.UI.Windows.Services
{
	public interface ISettingsStore
	{
		bool TryRead(string key);

		T Read<T>(string key, T defaultValue = default);

		void Write<T>(string key, T value);

		T ReadJson<T>(string key, T defaultValue = default);

		void WriteJson<T>(string key, T value);

		T ReadSecure<T>(string key, T defaultValue = default);

		void WriteSecure<T>(string key, T value);

		TEnum ReadEnum<TEnum>(string key, TEnum defaultValue)
			where TEnum : struct, Enum;

		void WriteEnum<TEnum>(string key, TEnum value)
			where TEnum : struct, Enum;
	}
}
