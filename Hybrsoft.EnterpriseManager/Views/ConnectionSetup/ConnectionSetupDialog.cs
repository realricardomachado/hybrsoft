using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Hybrsoft.EnterpriseManager.Configuration;
using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;

namespace Hybrsoft.EnterpriseManager.Views.ConnectionSetup
{
	public static class ConnectionSetupDialog
	{
		public static async Task<bool> ShowAsync(XamlRoot xamlRoot)
		{
			var resourceService = ServiceLocator.Current.GetService<IResourceService>();
			var view = new ConnectionSetupView();
			var dialog = new ContentDialog
			{
				Title = resourceService.GetString(ResourceFiles.UI, "ConnectionSetupDialog_Title"),
				Content = view,
				PrimaryButtonText = resourceService.GetString(ResourceFiles.UI, "ConnectionSetupDialog_SaveAndContinue"),
				CloseButtonText = resourceService.GetString(ResourceFiles.UI, "ConnectionSetupDialog_Cancel"),
				DefaultButton = ContentDialogButton.Primary,
				XamlRoot = xamlRoot
			};

			dialog.PrimaryButtonClick += async (_, args) =>
			{
				var deferral = args.GetDeferral();
				try
				{
					args.Cancel = !await view.SaveAsync();
				}
				finally
				{
					deferral.Complete();
				}
			};

			return await dialog.ShowAsync() == ContentDialogResult.Primary;
		}
	}
}
