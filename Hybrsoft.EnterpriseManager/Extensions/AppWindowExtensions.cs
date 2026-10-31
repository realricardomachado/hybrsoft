using Hybrsoft.UI.Windows.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Graphics;

namespace Hybrsoft.EnterpriseManager.Extensions
{
	public static class AppWindowExtensions
	{
		public static void SetDefaultIcon(this Window window)
		{
			window.AppWindow.SetIcon("Assets/default.ico");
		}

		public static async Task SetWindowPositionToCenterAsync(this Window window)
		{
			var displayList = await DeviceInformation.FindAllAsync(DisplayMonitor.GetDeviceSelector());
			if (displayList.Count == 0)
			{
				return;
			}

			var monitorInfo = await DisplayMonitor.FromInterfaceIdAsync(displayList[0].Id);
			var Height = monitorInfo.NativeResolutionInRawPixels.Height;
			var Width = monitorInfo.NativeResolutionInRawPixels.Width;

			AppWindow appWindow = window.AppWindow;
			var CenterPosition = appWindow.Position;
			CenterPosition.X = (Width - appWindow.Size.Width) / 2;
			CenterPosition.Y = (Height - appWindow.Size.Height) / 2;
			appWindow.Move(CenterPosition);
		}

		public static void SetWindowSize(this Window window, SizeInt32 size, ISettingsService settingsService)
		{
			window.AppWindow.Resize(size);
			window.AppWindow.Changed += (_, args) =>
			{
				if (args.DidSizeChange)
				{
					settingsService.WindowSize = window.AppWindow.Size;
				}
			};
		}
	}
}
