using Hybrsoft.EnterpriseManager.Common;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.EnterpriseManager.Extensions;
using Hybrsoft.UI.Windows.Services;
using Hybrsoft.UI.Windows.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hybrsoft.EnterpriseManager.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class LoginView : Page, IStartupView
	{
		private readonly TaskCompletionSource<bool> _initialization = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public LoginView()
		{
			ViewModel = ServiceLocator.Current.GetService<LoginViewModel>();
			InitializeContext();
			this.InitializeComponent();
			ModernTitleBar.InitializeTitleBar(WindowTracker.GetCurrentView().Window);
		}

		public LoginViewModel ViewModel { get; }
		public Task InitializationTask => _initialization.Task;

		private void InitializeContext()
		{
			var context = ServiceLocator.Current.GetService<IContextService>();
			var currentView = WindowTracker.GetCurrentView();
			context.Initialize(DispatcherQueue, (int)currentView.ID, currentView.IsMain);
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			try
			{
				_currentEffectMode = EffectMode.None;
				await ViewModel.LoadAsync(e.Parameter as ShellArgs);
				InitializeNavigation();
				_initialization.TrySetResult(true);
			}
			catch (Exception ex)
			{
				if (!_initialization.TrySetException(ex))
				{
					throw;
				}
			}
		}

		private void InitializeNavigation()
		{
			var navigationService = ServiceLocator.Current.GetService<INavigationService>();
			navigationService.Initialize(Frame);
		}

		protected override async void OnKeyDown(KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				DoEffectOut();
				await Task.Delay(100);
				ViewModel.Login();
			}
			base.OnKeyDown(e);
		}

		private async void OnShowLoginWithPassword(object sender, RoutedEventArgs e)
		{
			await Task.Delay(100);
			passwordView.Focus();
		}

		private void OnBackgroundFocus(object sender, RoutedEventArgs e)
		{
			DoEffectIn();
		}

		private void OnForegroundFocus(object sender, RoutedEventArgs e)
		{
			DoEffectOut();
		}

		private EffectMode _currentEffectMode = EffectMode.None;

		private void DoEffectIn(double milliseconds = 1000)
		{
			if (_currentEffectMode == EffectMode.Foreground || _currentEffectMode == EffectMode.None)
			{
				_currentEffectMode = EffectMode.Background;
				background.Scale(milliseconds, 1.0, 1.1);
				background.Blur(milliseconds, 6.0, 0.0);
				foreground.Scale(500, 1.0, 0.95);
				foreground.Fade(milliseconds, 1.0, 0.75);
			}
		}

		private void DoEffectOut(double milliseconds = 1000)
		{
			if (_currentEffectMode == EffectMode.Background || _currentEffectMode == EffectMode.None)
			{
				_currentEffectMode = EffectMode.Foreground;
				background.Scale(milliseconds, 1.1, 1.0);
				background.Blur(milliseconds, 0.0, 6.0);
				foreground.Scale(500, 0.95, 1.0);
				foreground.Fade(milliseconds, 0.75, 1.0);
			}
		}

		public enum EffectMode
		{
			None,
			Background,
			Foreground,
			Disabled
		}
	}
}
