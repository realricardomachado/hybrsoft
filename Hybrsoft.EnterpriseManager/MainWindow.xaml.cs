using Hybrsoft.EnterpriseManager.Common;
using Hybrsoft.EnterpriseManager.Configuration; //Used with SKIP_LOGIN
using Hybrsoft.EnterpriseManager.Extensions;
using Hybrsoft.EnterpriseManager.Services.Infrastructure;
using Hybrsoft.EnterpriseManager.Tools;
using Hybrsoft.EnterpriseManager.Views;
using Hybrsoft.UI.Windows.Infrastructure.Common;
using Hybrsoft.UI.Windows.Models; //Used with SKIP_LOGIN
using Hybrsoft.UI.Windows.Services; //Used with SKIP_LOGIN
using Hybrsoft.UI.Windows.ViewModels;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hybrsoft.EnterpriseManager
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private readonly TaskCompletionSource<bool> _activated = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public MainWindow(IActivatedEventArgs args)
		{
			this.InitializeComponent();
			WindowTracker.Register(this);
			ThemeExtensions.TrySetMicaBackdrop(this, true);
			Activated += OnActivated;

			InitializationTask = InitializeAsync(args);
		}

		public Task InitializationTask { get; }

		private async Task InitializeAsync(IActivatedEventArgs args)
		{
			var activationInfo = ActivationService.GetActivationInfo(args);
			var shellArgs = new ShellArgs
			{
				ViewModel = activationInfo.EntryViewModel,
				Parameter = activationInfo.EntryArgs,
				UserInfo = await TryGetUserInfoAsync(args as IActivatedEventArgsWithUser)
			};
#if SKIP_LOGIN
			var UserService = ServiceLocator.Current.GetService<IUserService>();
			UserModel user = await UserService.GetUserByEmailAsync(shellArgs.UserInfo.AccountName);
			if (user is not null)
			{
				AppSettings.Current.UserID = user.UserID;
				var LookupTables = ServiceLocator.Current.GetService<ILookupTables>();
				await LookupTables.LoadAfterLoginAsync();
			}
			rootFrame.Navigate(typeof(MainShellView), shellArgs);
			var loginService = ServiceLocator.Current.GetService<ILoginService>();
			loginService.IsAuthenticated = true;
			await WaitForStartupViewInitializationAsync();
#else
			rootFrame.Navigate(typeof(LoginView), shellArgs);
			await WaitForStartupViewInitializationAsync();
#endif
			await _activated.Task;
		}

		private async Task WaitForStartupViewInitializationAsync()
		{
			if (rootFrame.Content is not IStartupView startupView)
			{
				throw new InvalidOperationException("The initial page does not expose its initialization task.");
			}

			await startupView.InitializationTask;
		}

		private void OnActivated(object sender, WindowActivatedEventArgs args)
		{
			if (args.WindowActivationState != WindowActivationState.Deactivated)
			{
				_activated.TrySetResult(true);
				Activated -= OnActivated;
			}
		}

		private static async Task<UserInfo> TryGetUserInfoAsync(IActivatedEventArgsWithUser argsWithUser)
		{
			if (argsWithUser != null)
			{
				var user = argsWithUser.User;
				var userInfo = new UserInfo
				{
					AccountName = await user.GetPropertyAsync(KnownUserProperties.AccountName) as String,
					FirstName = await user.GetPropertyAsync(KnownUserProperties.FirstName) as String,
					LastName = await user.GetPropertyAsync(KnownUserProperties.LastName) as String
				};
				if (!userInfo.IsEmpty)
				{
					if (string.IsNullOrEmpty(userInfo.AccountName))
					{
						userInfo.AccountName = $"{userInfo.FirstName} {userInfo.LastName}";
					}
					var pictureStream = await user.GetPictureAsync(UserPictureSize.Size64x64);
					if (pictureStream != null)
					{
						userInfo.PictureSource = await BitmapTools.LoadBitmapAsync(pictureStream);
					}
					return userInfo;
				}
			}
			return UserInfo.Default;
		}
	}
}
