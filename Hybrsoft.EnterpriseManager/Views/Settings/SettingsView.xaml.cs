using Hybrsoft.UI.Windows.ViewModels;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.EnterpriseManager.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hybrsoft.EnterpriseManager.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsView : Page
	{
		public SettingsView()
		{
			ViewModel = ServiceLocator.Current.GetService<SettingsViewModel>();
			this.InitializeComponent();
		}

		public SettingsViewModel ViewModel { get; }

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			await ViewModel.LoadAsync(e.Parameter as SettingsArgs);
		}

		private async void ConfigureConnectionButton_Click(object sender, RoutedEventArgs e)
		{
			if (await ConnectionSetupDialog.ShowAsync(Content.XamlRoot, isStartup: false))
			{
				ViewModel.NotifyConnectionSaved();
			}
		}
	}
}
