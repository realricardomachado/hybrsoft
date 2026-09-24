using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Infrastructure.Common;
using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Hybrsoft.UI.Windows.Models;
using Hybrsoft.UI.Windows.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hybrsoft.UI.Windows.ViewModels
{
	public partial class LoginViewModel(
		ILoginService loginService,
		ISettingsService settingsService,
		ILicenseService licenseService,
		INetworkService networkService,
		ILookupTables lookupTables,
		ICommonServices commonServices) : ViewModelBase(commonServices)
	{
		private readonly ILoginService _loginService = loginService;
		private readonly ISettingsService _settingsService = settingsService;
		private readonly ILicenseService _licenseService = licenseService;
		private readonly INetworkService _networkService = networkService;
		private readonly ILookupTables _lookupTables = lookupTables;

		private ShellArgs ViewModelArgs { get; set; }

		private bool _isBusy = false;
		public bool IsBusy
		{
			get { return _isBusy; }
			set { Set(ref _isBusy, value); }
		}

		private bool _isLoginWithPassword = false;
		public bool IsLoginWithPassword
		{
			get { return _isLoginWithPassword; }
			set { Set(ref _isLoginWithPassword, value); }
		}

		private bool _isLoginWithWindowsHello = false;
		public bool IsLoginWithWindowsHello
		{
			get { return _isLoginWithWindowsHello; }
			set { Set(ref _isLoginWithWindowsHello, value); }
		}

		private string _userName = null;
		public string UserName
		{
			get { return _userName; }
			set { Set(ref _userName, value); }
		}

		private string _password;
		public string Password
		{
			get { return _password; }
			set { Set(ref _password, value); }
		}

		private bool _hasSecurityAdministration;
		private bool _isLicenseValid;

		public ICommand ShowLoginWithPasswordCommand => new RelayCommand(ShowLoginWithPassword);
		public ICommand LoginWithPasswordCommand => new RelayCommand(LoginWithPassword);
		public ICommand LoginWithWindowsHelloCommand => new RelayCommand(LoginWithWindowsHello);

		public async Task<Task> LoadAsync(ShellArgs args)
		{
			ViewModelArgs = args;

			UserName = _settingsService.UserName ?? args.UserInfo.AccountName;
			IsLoginWithWindowsHello = await _loginService.IsWindowsHelloEnabledAsync(UserName);
			IsLoginWithPassword = !IsLoginWithWindowsHello;
			IsBusy = false;

			return Task.CompletedTask;
		}

		public void Login()
		{
			if (IsLoginWithPassword)
			{
				LoginWithPassword();
			}
			else
			{
				LoginWithWindowsHello();
			}
		}

		private void ShowLoginWithPassword()
		{
			IsLoginWithWindowsHello = false;
			IsLoginWithPassword = true;
		}

		public async void LoginWithPassword()
		{
			IsBusy = true;
			var result = ValidateInput();
			if (result.IsOk)
			{
				result = await _loginService.SignInWithPasswordAsync(UserName, Password);
				if (result.IsOk)
				{
					await LoadPermissionsAsync();
					if (!await _loginService.IsWindowsHelloEnabledAsync(UserName))
					{
						await _loginService.TrySetupWindowsHelloAsync(UserName);
					}
					// It can only be assigned to the settings after verification in IsWindowsHelloEnabledAsync.
					// Where it will be detected in case the user entered at login is different from the last logged-in user.
					_settingsService.UserName = UserName;
					await VerifyLicenseAsync();
					await EnterApplication();
					return;
				}
			}
			await DialogService.ShowAsync(result.Message, result.Description);
			IsBusy = false;
		}

		public async void LoginWithWindowsHello()
		{
			IsBusy = true;
			var result = await _loginService.SignInWithWindowsHelloAsync();
			if (result.IsOk)
			{
				await LoadPermissionsAsync();
				await VerifyLicenseAsync();
				await EnterApplication();
				return;
			}
			await DialogService.ShowAsync(result.Message, result.Description);
			IsBusy = false;
		}

		private async Task EnterApplication()
		{
			if (ViewModelArgs.UserInfo.AccountName != UserName)
			{
				ViewModelArgs.UserInfo = new UserInfo
				{
					AccountName = UserName,
					FirstName = _settingsService.UserFirstName,
					LastName = _settingsService.UserLastName,
					PictureSource = null
				};
			}
			bool canAccessViewModel = AuthorizationService.CanAccessViewModel(ViewModelArgs.ViewModel);
			if (!canAccessViewModel)
			{
				Type defaultViewModel = AuthorizationService.GetDefaultAccessibleViewModel();
				if (defaultViewModel is not null)
				{
					ViewModelArgs.ViewModel = defaultViewModel;
					ViewModelArgs.Parameter = null;
				}
				else
				{
					string title = ResourceService.GetString<LoginViewModel>(ResourceFiles.Errors, "AccessDenied");
					string description = ResourceService.GetString<LoginViewModel>(ResourceFiles.Errors, "NoAccessibleFeatures");
					await DialogService.ShowAsync(title, description);
					IsBusy = false;
					return;
				}
			}
			if (_hasSecurityAdministration || _isLicenseValid)
			{
				NavigationService.Navigate<MainShellViewModel>(ViewModelArgs);
			}
			else
			{
				ViewModelArgs.Parameter = new LicenseActivationModel
				{
					Email = UserName,
					Password = Password,
					ProductType = AppType.EnterpriseManager
				};
				NavigationService.Navigate<LicenseActivationViewModel>(ViewModelArgs);
			}
		}

		private Result ValidateInput()
		{
			if (string.IsNullOrWhiteSpace(UserName))
			{
				string message = ResourceService.GetString<LoginViewModel>(ResourceFiles.Errors, "LoginError");
				string description = ResourceService.GetString<LoginViewModel>(ResourceFiles.Errors, "PleaseEnterValidUsername");
				return Result.Error(message, description);
			}
			if (string.IsNullOrWhiteSpace(Password))
			{
				string message = ResourceService.GetString<LoginViewModel>(ResourceFiles.Errors, "LoginError");
				string description = ResourceService.GetString<LoginViewModel>(ResourceFiles.Errors, "PleaseEnterValidPassword");
				return Result.Error(message, description);
			}
			return Result.Ok();
		}

		private async Task LoadPermissionsAsync()
		{
			await _lookupTables.LoadAfterLoginAsync();
			_hasSecurityAdministration = AuthorizationService.HasSecurityAdministrationPermission;
		}

		private async Task VerifyLicenseAsync()
		{
			if (!_hasSecurityAdministration)
			{
				_isLicenseValid = await IsLicenseValidAsync();
			}
		}

		private async Task<bool> IsLicenseValidAsync()
		{
			if (await _licenseService.IsLicenseValidOfflineAsync())
			{
				return true;
			}
			if (await _networkService.IsInternetAvailableAsync())
			{
				var license = await _licenseService.ValidateSubscriptionOnlineAsync(UserName, Password);
				if (license.IsActivated)
				{
					_licenseService.SaveLicenseLocally(_licenseService.CreateSubscriptionInfoDto(license));
					return true;
				}
			}
			return false;
		}
	}
}
