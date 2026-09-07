using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.EnterpriseManager.Extensions;
using Hybrsoft.EnterpriseManager.Views.SplashScreen;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hybrsoft.EnterpriseManager
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application, IDisposable
	{
		private Window m_window;
		private ExtendedSplash splash_Screen;
		private bool disposedValue;

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();

			this.UnhandledException += OnUnhandledException;
		}

		/// <summary>
		/// Invoked when the application is launched.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs args)
		{
			AppActivationArguments activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
			await ActivateAsync(activatedArgs.Data as Windows.ApplicationModel.Activation.IActivatedEventArgs);
		}

		/// <summary>
		/// Invoked when the application is activated.
		/// </summary>
		private async Task ActivateAsync(Windows.ApplicationModel.Activation.IActivatedEventArgs args)
		{
			splash_Screen = new ExtendedSplash();
			splash_Screen.SetDefaultWindowSize();
			await splash_Screen.SetWindowPositionToCenterAsync();
			splash_Screen.Activate();
			splash_Screen.Closed += (_, _) => { ServiceLocator.DisposeByViewID(splash_Screen.AppWindow.Id.Value); };
			await Task.Delay(2000);

			m_window = new MainWindow(args);
			m_window.SetDefaultIcon();
			m_window.SetDefaultWindowSize();
			await m_window.SetWindowPositionToCenterAsync();
			m_window.Activate();
			m_window.Closed += async (_, _) =>
			{
				var logService = ServiceLocator.Current.GetService<ILogService>();
				await logService.WriteAsync(LogType.Information, "App", "Closing", "Application End", $"Application ended by '{AppSettings.Current.UserName}'.");
			};

			await Task.Delay(200);
			splash_Screen.Close();
		}

		/// <summary>
		/// Invoked when the application is unhandled exception.
		/// </summary>
		private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
		{
			var logService = ServiceLocator.Current.GetService<ILogService>();
			logService.WriteAsync(LogType.Error, "App", "UnhandledException", e.Message, e.Exception.ToString());
		}

		#region Dispose
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing){ }
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
