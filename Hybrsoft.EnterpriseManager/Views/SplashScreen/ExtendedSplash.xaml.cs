using Hybrsoft.EnterpriseManager.Common;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.EnterpriseManager.Views.ConnectionSetup;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hybrsoft.EnterpriseManager.Views.SplashScreen
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ExtendedSplash : Window
	{
		public ExtendedSplash()
		{
			this.InitializeComponent();
			WindowTracker.Register(this);

			OverlappedPresenter presenter = this.AppWindow.Presenter as OverlappedPresenter;
#if !DEBUG
			presenter.IsAlwaysOnTop = true;
#endif
			presenter.IsMaximizable = false;
			presenter.IsMinimizable = false;
			presenter.IsResizable = false;
			this.AppWindow.IsShownInSwitchers = false;
			this.Title = "";
			presenter.SetBorderAndTitleBar(false, false);

		}

		public void ReportProgress(StartupProgress progress)
		{
			ProgressBar.IsIndeterminate = progress.IsIndeterminate;
			if (!progress.IsIndeterminate)
			{
				ProgressBar.Value = progress.Value;
			}
			StatusTextBlock.Text = progress.Message;
		}

		public Task<bool> ShowConnectionSetupAsync()
		{
			return ConnectionSetupDialog.ShowAsync(Content.XamlRoot);
		}
	}
}
