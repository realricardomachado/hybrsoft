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
			AppWindow appWindow = window.AppWindow;
			appWindow.Resize(size);
			window.AppWindow.Changed += (_, args) =>
			{
				if (appWindow.Presenter is not OverlappedPresenter presenter)
				{
					return;
				}

				if (args.DidSizeChange && presenter.State == OverlappedPresenterState.Restored)
				{
					settingsService.WindowSize = appWindow.Size;
				}

				if (args.DidPresenterChange)
				{
					if (presenter.State == OverlappedPresenterState.Maximized)
					{
						settingsService.IsWindowMaximized = true;
					}
					else if (presenter.State == OverlappedPresenterState.Restored)
					{
						settingsService.IsWindowMaximized = false;
					}
				}
			};
		}

		public static void SetWindowMaximized(this Window window, bool isMaximized)
		{
			if (isMaximized && window.AppWindow.Presenter is OverlappedPresenter presenter)
			{
				presenter.Maximize();
			}
		}
	}
}
