using Hybrsoft.EnterpriseManager.Common;
using Hybrsoft.EnterpriseManager.Extensions;
using Hybrsoft.EnterpriseManager.Services.DataServiceFactory;
using Hybrsoft.EnterpriseManager.Views;
using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.Models;
using Hybrsoft.UI.Windows.Models;
using Hybrsoft.UI.Windows.Services;
using Hybrsoft.UI.Windows.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Services
{
	public partial class NavigationService : INavigationService
	{
		private static readonly ConcurrentDictionary<Type, Type> _viewModelMap = new();

		public NavigationService(
			IDataServiceFactory dataServiceFactory,
			IResourceService resourceService,
			ISettingsService settingsService)
		{
			MainViewId = (int)WindowTracker.MainID;
			_dataServiceFactory = dataServiceFactory;
			_resourceService = resourceService;
			_settingsService = settingsService;
		}

		private readonly IDataServiceFactory _dataServiceFactory;
		private readonly IResourceService _resourceService;
		private readonly ISettingsService _settingsService;

		public int MainViewId { get; }

		static public void Register<TViewModel, TView>() where TView : Page
		{
			if (!_viewModelMap.TryAdd(typeof(TViewModel), typeof(TView)))
			{
				throw new InvalidOperationException($"ViewModel already registered '{typeof(TViewModel).FullName}'");
			}
		}

		static public Type GetView<TViewModel>()
		{
			return GetView(typeof(TViewModel));
		}
		static public Type GetView(Type viewModel)
		{
			if (_viewModelMap.TryGetValue(viewModel, out Type view))
			{
				return view;
			}
			throw new InvalidOperationException($"View not registered for ViewModel '{viewModel.FullName}'");
		}

		static public Type GetViewModel(Type view)
		{
			var type = _viewModelMap.Where(r => r.Value == view).Select(r => r.Key).FirstOrDefault();
			return type ?? throw new InvalidOperationException($"View not registered for ViewModel '{view.FullName}'");
		}

		public bool IsMainView => WindowTracker.GetCurrentView().IsMain;

		public Frame Frame { get; private set; }

		public bool CanGoBack => Frame.CanGoBack;

		public void GoBack() => Frame.GoBack();

		public bool CanLogoff => Frame.BackStack.Last().SourcePageType.Name == nameof(LoginView);

		public void Initialize(object frame)
		{
			Frame = frame as Frame;
		}

		public bool Navigate<TViewModel>(object parameter = null)
		{
			return Navigate(typeof(TViewModel), parameter);
		}
		public bool Navigate(Type viewModelType, object parameter = null)
		{
			if (Frame == null)
			{
				throw new InvalidOperationException("Navigation frame not initialized.");
			}
			return Frame.Navigate(GetView(viewModelType), parameter);
		}

		public async Task<int> CreateNewViewAsync<TViewModel>(object parameter = null)
		{
			return await CreateNewViewAsync(typeof(TViewModel), parameter);
		}
		public async Task<int> CreateNewViewAsync(Type viewModelType, object parameter = null)
		{
			Window newWindow = new();
			WindowTracker.Register(newWindow);
			newWindow.SetDefaultIcon();
			newWindow.SetWindowSize(_settingsService.WindowSize, _settingsService);
			await newWindow.SetWindowPositionToCenterAsync();

			var frame = new Frame();
			var args = new ShellArgs
			{
				ViewModel = viewModelType,
				Parameter = parameter
			};
			frame.Navigate(typeof(ShellView), args);
			newWindow.Content = frame;

			await Task.Delay(200);
			newWindow.Activate();
			ThemeExtensions.TrySetMicaBackdrop(newWindow, true);

			return (int)newWindow.AppWindow.Id.Value;
		}

		public void CloseView()
		{
			var currentView = WindowTracker.GetCurrentView();
			currentView?.Window.Close();
		}

		public IEnumerable<NavigationItemModel> GetItems()
		{
			using var dataService = _dataServiceFactory.CreateDataService();
			return GetNavigationItemByParentId(dataService.GetNavigationItemByAppType(AppType.EnterpriseManager), null);
		}

		private IEnumerable<NavigationItemModel> GetNavigationItemByParentId(IList<NavigationItem> items, int? parentId)
		{
			return items.Where(f => f.ParentID == parentId)
				.Select(f => new NavigationItemModel(
					string.IsNullOrEmpty(f.Uid) ? f.Label : _resourceService.GetString(ResourceFiles.UI, f.Uid),
					f.Icon.Value,
					f.ViewModel,
					f.ParentID,
					[.. GetNavigationItemByParentId(items, f.NavigationItemID)],
					string.IsNullOrEmpty(f.ViewModel) ? null : GetTypeViewModelByName(f.ViewModel)));
		}

		private static Type GetTypeViewModelByName(string view) => view switch
		{
			nameof(DashboardViewModel) => typeof(DashboardViewModel),
			nameof(RelativesViewModel) => typeof(RelativesViewModel),
			nameof(StudentsViewModel) => typeof(StudentsViewModel),
			nameof(ClassroomsViewModel) => typeof(ClassroomsViewModel),
			nameof(DismissibleStudentsViewModel) => typeof(DismissibleStudentsViewModel),
			nameof(DismissalsViewModel) => typeof(DismissalsViewModel),
			nameof(LostAndFoundsViewModel) => typeof(LostAndFoundsViewModel),
			nameof(CompaniesViewModel) => typeof(CompaniesViewModel),
			nameof(SubscriptionsViewModel) => typeof(SubscriptionsViewModel),
			nameof(PermissionsViewModel) => typeof(PermissionsViewModel),
			nameof(RolesViewModel) => typeof(RolesViewModel),
			nameof(UsersViewModel) => typeof(UsersViewModel),
			nameof(AppLogsViewModel) => typeof(AppLogsViewModel),
			_ => throw new ArgumentOutOfRangeException(nameof(view), $"Not expected view value: {view}"),
		};
	}
}
