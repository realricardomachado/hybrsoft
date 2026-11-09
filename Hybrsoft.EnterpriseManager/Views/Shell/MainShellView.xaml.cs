using Hybrsoft.EnterpriseManager.Common;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.EnterpriseManager.Services;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Models;
using Hybrsoft.UI.Windows.Services;
using Hybrsoft.UI.Windows.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hybrsoft.EnterpriseManager.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainShellView : Page, IStartupView
	{
		private INavigationService _navigationService = null;
		private readonly TaskCompletionSource<bool> _initialization = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public MainShellView()
		{
			ViewModel = ServiceLocator.Current.GetService<MainShellViewModel>();
			InitializeContext();
			this.InitializeComponent();
			ModernTitleBar.InitializeTitleBar(WindowTracker.GetCurrentView().Window);
			InitializeNavigation();
		}

		public MainShellViewModel ViewModel { get; }
		public Task InitializationTask => _initialization.Task;

		private void InitializeContext()
		{
			var context = ServiceLocator.Current.GetService<IContextService>();
			var currentView = WindowTracker.GetCurrentView();
			context.Initialize(DispatcherQueue, (int)currentView.ID, currentView.IsMain);
		}

		private void InitializeNavigation()
		{
			_navigationService = ServiceLocator.Current.GetService<INavigationService>();
			_navigationService.Initialize(frame);
			frame.Navigated += OnFrameNavigated;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			try
			{
				await ViewModel.LoadAsync(e.Parameter as ShellArgs);
				ViewModel.Subscribe();
				_initialization.TrySetResult(true);
				await ViewModel.ShowLicenseExpirationWarningAsync();
			}
			catch (Exception ex)
			{
				if (!_initialization.TrySetException(ex))
				{
					throw;
				}
			}
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			ViewModel.Unload();
			ViewModel.Unsubscribe();
		}

		private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.SelectedItem is NavigationItemModel item)
			{
				if (item.ViewModel != null)
				{
					ViewModel.NavigateTo(item.ViewModel);
				}
			}
			else if (args.IsSettingsSelected)
			{
				ViewModel.NavigateTo(typeof(SettingsViewModel));
			}
			UpdateBackButton();
		}

		private void OnFrameNavigated(object sender, NavigationEventArgs e)
		{
			var targetType = NavigationService.GetViewModel(e.SourcePageType);
			ViewModel.SelectedItem = targetType.Name switch
			{
				"SettingsViewModel" => NavigationView.SettingsItem,
				_ => ViewModel.NavigationItems.Where(r => r.ViewModel == targetType).FirstOrDefault(),
			};
			UpdateBackButton();
		}

		private void UpdateBackButton()
		{
			ModernTitleBar.IsBackButtonEnabled = _navigationService.CanGoBack;
		}

		private async void OnLogoff(object sender, RoutedEventArgs e)
		{
			var resourceService = ServiceLocator.Current.GetService<IResourceService>();
			string title = resourceService.GetString(ResourceFiles.UI, "ContentDialog_Title_ConfirmLogoff");
			string content = resourceService.GetString(ResourceFiles.Questions, "AreYouSureYouWantToLogoff");
			string cancel = resourceService.GetString(ResourceFiles.UI, "ContentDialog_CloseButtonText_Cancel");
			var dialogService = ServiceLocator.Current.GetService<IDialogService>();
			if (await dialogService.ShowAsync(title, content, "Ok", cancel))
			{
				var loginService = ServiceLocator.Current.GetService<ILoginService>();
				loginService.Logoff();
				while (Frame.CanGoBack)
				{
					Frame.GoBack();
				}
			}
		}
	}
}
