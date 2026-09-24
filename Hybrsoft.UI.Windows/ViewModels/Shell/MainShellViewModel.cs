using Hybrsoft.Enums;
using Hybrsoft.Infrastructure.Common;
using Hybrsoft.Infrastructure.Models;
using Hybrsoft.UI.Windows.Models;
using Hybrsoft.UI.Windows.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hybrsoft.UI.Windows.ViewModels
{
	public partial class MainShellViewModel(ILicenseService licenseService, ICommonServices commonServices) : ShellViewModel(commonServices)
	{
		private readonly ILicenseService _licenseService = licenseService;

		private object _selectedItem;
		public object SelectedItem
		{
			get => _selectedItem;
			set => Set(ref _selectedItem, value);
		}

		private bool _isPaneOpen = true;
		public bool IsPaneOpen
		{
			get => _isPaneOpen;
			set => Set(ref _isPaneOpen, value);
		}

		private IEnumerable<NavigationItemModel> _navigationItems;
		public IEnumerable<NavigationItemModel> NavigationItems
		{
			get => _navigationItems;
			set => Set(ref _navigationItems, value);
		}

		public override async Task LoadAsync(ShellArgs args)
		{
			NavigationItems = [.. GetItems()];
			await base.LoadAsync(args);
			await UpdateAppLogBadge();
		}

		public async Task ShowLicenseExpirationWarningAsync()
		{
			if (!AuthorizationService.HasSecurityAdministrationPermission)
			{
				int? remainingDays = await _licenseService.GetRemainingDaysAsync();
				if (remainingDays.HasValue && remainingDays.Value <= 5)
				{
					string title = ResourceService.GetString<MainShellViewModel>(ResourceFiles.Warnings, "LicenseExpiringSoon");
					string message = string.Format(ResourceService.GetString<MainShellViewModel>(ResourceFiles.Warnings, "YouLicenseWillExpireIn0Days"), remainingDays.Value);
					await Task.Delay(3000);
					WarningMessageYourself(title, message);
				}
			}
		}

		override public void Subscribe()
		{
			MessageService.Subscribe<ILogService, AppLog>(this, OnLogServiceMessage);
			base.Subscribe();
		}

		override public void Unsubscribe()
		{
			base.Unsubscribe();
		}

		public override void Unload()
		{
			base.Unload();
		}

		public async void NavigateTo(Type viewModel)
		{
			switch (viewModel.Name)
			{
				case nameof(DashboardViewModel):
					NavigationService.Navigate(viewModel);
					break;
				case nameof(RelativesViewModel):
					NavigationService.Navigate(viewModel, new RelativeListArgs());
					break;
				case nameof(StudentsViewModel):
					NavigationService.Navigate(viewModel, new StudentListArgs());
					break;
				case nameof(ClassroomsViewModel):
					NavigationService.Navigate(viewModel, new ClassroomListArgs());
					break;
				case nameof(DismissibleStudentsViewModel):
					NavigationService.Navigate(viewModel, new DismissibleStudentListArgs());
					break;
				case nameof(DismissalsViewModel):
					NavigationService.Navigate(viewModel, new DismissalListArgs());
					break;
				case nameof(LostAndFoundsViewModel):
					NavigationService.Navigate(viewModel, new LostAndFoundListArgs());
					break;
				case nameof(CompaniesViewModel):
					NavigationService.Navigate(viewModel, new CompanyListArgs());
					break;
				case nameof(SubscriptionsViewModel):
					NavigationService.Navigate(viewModel, new SubscriptionListArgs());
					break;
				case nameof(PermissionsViewModel):
					NavigationService.Navigate(viewModel, new PermissionListArgs());
					break;
				case nameof(RolesViewModel):
					NavigationService.Navigate(viewModel, new RoleListArgs());
					break;
				case nameof(UsersViewModel):
					NavigationService.Navigate(viewModel, new UserListArgs());
					break;
				case nameof(AppLogsViewModel):
					NavigationService.Navigate(viewModel, new AppLogListArgs());
					await LogService.MarkAllAsReadAsync();
					await UpdateAppLogBadge();
					break;
				case nameof(SettingsViewModel):
					NavigationService.Navigate(viewModel, new SettingsArgs());
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(viewModel), $"Not expected view value: {viewModel.Name}");
			}
		}

		private IEnumerable<NavigationItemModel> GetItems()
		{
			return NavigationService.GetItems().Where(HasUserPermission);
		}

		private bool HasUserPermission(NavigationItemModel item)
		{
			if (item.ViewModel is null && item.Children != null)
			{
				if (item.Children.Count == 0)
				{
					return true;
				}
				var validChildren = item.Children.Where(c => HasUserPermission(c));
				item.Children = [.. validChildren];
				return item.Children.Any();
			}
			return AuthorizationService.CanAccessViewModel(item.ViewModel);
		}

		private async void OnLogServiceMessage(ILogService logService, string message, AppLog log)
		{
			if (message == "LogAdded")
			{
				await ContextService.RunAsync(async () =>
				{
					await UpdateAppLogBadge();
				});
			}
		}

		private async Task UpdateAppLogBadge()
		{
			var request = new DataRequest<AppLog> { Where = r => !r.IsRead && r.AppType == AppType.EnterpriseManager };
			int count = await LogService.GetLogsCountAsync(request);
			var appLogsItem = NavigationItems.FirstOrDefault(f => f.Tag == nameof(AppLogsViewModel));
			appLogsItem?.Badge = count > 0
				? new Microsoft.UI.Xaml.Controls.InfoBadge
				{
					Style = (Style)Application.Current.Resources["CriticalValueInfoBadgeStyle"],
					Value = count
				}
				: null;
		}
	}
}
