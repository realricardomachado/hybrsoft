using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.UI.Windows.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Controls
{
	public sealed partial class ConnectionSetupView : UserControl
	{
		public ConnectionSetupViewModel ViewModel { get; }

		public ConnectionSetupView()
		{
			ViewModel = ServiceLocator.Current.GetService<ConnectionSetupViewModel>();
			this.InitializeComponent();
		}

		public Task<bool> SaveAsync()
		{
			return ViewModel.SaveAsync();
		}
	}
}
