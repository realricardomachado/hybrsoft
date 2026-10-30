using Hybrsoft.DTOs;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Models;
using Hybrsoft.UI.Windows.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	public class LicenseService(
		IUserService userService,
		INetworkService networkService,
		ISettingsStore settingsStore,
		ILogService logService) : ILicenseService
	{
		private const string _licenseData = "LicenseData";
		private const string _lastLicenseSync = "LastLicenseSync";
		private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
		private string _jwtToken;

		private readonly IUserService _userService = userService;
		private readonly INetworkService _networkService = networkService;
		private readonly ISettingsStore _settingsStore = settingsStore;
		private readonly ILogService _logService = logService;

		public async Task<LicenseResponse> ActivateSubscriptionOnlineAsync(LicenseActivationModel license)
		{
			try
			{
				_jwtToken = await AuthenticateAsync(license.Email, license.Password);
				if (string.IsNullOrEmpty(_jwtToken))
				{
					return new LicenseResponse { IsActivated = false, Message = "Authentication failed." };
				}

				var client = _networkService.GetHttpClient(_jwtToken);
				var payload = new { license.Email, license.LicenseKey, ProductType = AppType.EnterpriseManager };
				var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
				var response = await client.PostAsync(AppConfig.ApiBaseUrl + "/subscription/activate", content);
				var json = await response.Content.ReadAsStringAsync();
				return JsonSerializer.Deserialize<LicenseResponse>(json, _jsonOptions);
			}
			catch (Exception ex)
			{
				await _logService.WriteAsync(LogType.Error, "License", "Activate", ex.Message, ex.ToString());
				return new LicenseResponse { IsActivated = false };
			}
		}

		public async Task<LicenseResponse> ValidateSubscriptionOnlineAsync(string email, string password)
		{
			try
			{
				_jwtToken = await AuthenticateAsync(email, password);
				if (string.IsNullOrEmpty(_jwtToken))
				{
					return new LicenseResponse { IsActivated = false, Message = "Authentication failed." };
				}

				var client  = _networkService.GetHttpClient(_jwtToken);
				var payload = new { Email = email, ProductType = AppType.EnterpriseManager };
				var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
				var response = await client.PostAsync(AppConfig.ApiBaseUrl + "/subscription/validate", content);
				if (response.IsSuccessStatusCode)
				{
					var json = await response.Content.ReadAsStringAsync();
					return JsonSerializer.Deserialize<LicenseResponse>(json, _jsonOptions);
				}
				return new LicenseResponse { IsActivated = false };
			}
			catch (Exception ex)
			{
				await _logService.WriteAsync(LogType.Error, "License", "Validate Online", ex.Message, ex.ToString());
				return new LicenseResponse { IsActivated = false };
			}
		}

		public void SaveLicenseLocally(SubscriptionInfoDto model)
		{
			_settingsStore.WriteSecure(_licenseData, model);
			_settingsStore.Write(_lastLicenseSync, DateTimeOffset.Now.ToString("o"));
		}

		public async Task<bool> IsLicenseValidOfflineAsync()
		{
			var license = _settingsStore.ReadSecure<SubscriptionInfoDto>(_licenseData);
			var lastSyncStr = _settingsStore.Read<string>(_lastLicenseSync);

			if (license is null || string.IsNullOrEmpty(lastSyncStr))
				return false;

			if (DateTimeOffset.TryParse(lastSyncStr, out DateTimeOffset lastSync))
			{
				// Allow offline use for 7 days
				if ((DateTimeOffset.Now - lastSync).TotalDays > 7)
					return false;

				try
				{
					return license.IsActivated && license.ExpirationDate > DateTimeOffset.Now;
				}
				catch
				{
					return false;
				}
			}
			return false;
		}

		public SubscriptionInfoDto CreateSubscriptionInfoDto(LicenseResponse source)
		{
			return new SubscriptionInfoDto
			{
				IsActivated = source.IsActivated,
				StartDate = source.StartDate,
				ExpirationDate = source.ExpirationDate,
				LicenseData = source.LicenseData,
				LicensedTo = source.LicensedTo,
				Message = source.Message,
			};
		}

		private async Task<string> AuthenticateAsync(string email, string password)
		{
			try
			{
				if (IsTokenValid())
				{
					return AppSettings.Current.JwtToken;
				}

				bool isPasswordEncrypted = false;
				if (string.IsNullOrEmpty(password))
				{
					var user = await _userService.GetUserByEmailAsync(email, true);
					var parts = user?.Password?.Split('-');
					password = parts?.Length > 1 ? parts[0] : user?.Password;
					isPasswordEncrypted = true;
				}

				var client = _networkService.GetHttpClient();
				var payload = new
				{
					Username = email,
					Password = password,
					IsEncrypted = isPasswordEncrypted
				};
				var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
				var response = await client.PostAsync(AppConfig.ApiBaseUrl + "/auth/login", content);
				var json = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<AuthenticateResponse>(json, _jsonOptions);

				AppSettings.Current.JwtToken = result?.Token;
				var handler = new JwtSecurityTokenHandler();
				var jwt = handler.ReadJwtToken(result?.Token);
				AppSettings.Current.JwtTokenExpiration = jwt.ValidTo.ToLocalTime();

				return result?.Token;
			}
			catch (Exception ex)
			{
				await _logService.WriteAsync(LogType.Error, "License", "Authenticate", ex.Message, ex.ToString());
				return null;
			}
		}

		private static bool IsTokenValid()
		{
			return !string.IsNullOrEmpty(AppSettings.Current.JwtToken)
				&& AppSettings.Current.JwtTokenExpiration > DateTime.Now;
		}

		public async Task<int?> GetRemainingDaysAsync()
		{
			try
			{
				var license = _settingsStore.ReadSecure<SubscriptionInfoDto>(_licenseData);

				if (license is null)
					return null;

				if (!license.IsActivated || license.ExpirationDate <= DateTimeOffset.Now)
					return 0;

				var remainingDays = (license.ExpirationDate.Value - DateTimeOffset.Now).Days;
				return remainingDays;
			}
			catch
			{
				return null;
			}
		}
	}
}
