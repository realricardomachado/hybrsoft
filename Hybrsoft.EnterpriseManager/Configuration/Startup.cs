using Hybrsoft.UI.Windows.Services;
using Hybrsoft.UI.Windows.ViewModels;
using Hybrsoft.EnterpriseManager.Services;
using Hybrsoft.EnterpriseManager.Views;
using Hybrsoft.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Hybrsoft.EnterpriseManager.Configuration
{
	static public class Startup
	{
		static private readonly ServiceCollection _serviceCollection = new();

		static public async Task InitializeCoreAsync(IProgress<StartupProgress> progress = null)
		{
			progress?.Report(new(10, "Preparing services"));
			ServiceLocator.Configure(_serviceCollection);

			AppConfig.Initialize();

			ConfigureNavigation();

			progress?.Report(new(30, "Initializing resources"));
			var resourceService = ServiceLocator.Current.GetService<IResourceService>();
			await resourceService.InitializeAsync();

			progress?.Report(new(40, resourceService.GetString(ResourceFiles.InfoMessages, "Startup_CheckingConfiguration")));
		}

		static public async Task InitializeDatabaseAsync(IProgress<StartupProgress> progress = null)
		{
			var resourceService = ServiceLocator.Current.GetService<IResourceService>();
			var logService = ServiceLocator.Current.GetService<ILogService>();
			await logService.WriteAsync(LogType.Information, "Startup", "Configuration", "Application Start", $"Application started.");

			progress?.Report(new(70, resourceService.GetString(ResourceFiles.InfoMessages, "Startup_LoadingLookupData")));
			await ConfigureLookupTables();
			progress?.Report(new(100, resourceService.GetString(ResourceFiles.InfoMessages, "Startup_Completed")));
		}

		private static void ConfigureNavigation()
		{
			NavigationService.Register<LicenseActivationViewModel, LicenseActivationView>();

			NavigationService.Register<ShellViewModel, ShellView>();
			NavigationService.Register<MainShellViewModel, MainShellView>();

			NavigationService.Register<DashboardViewModel, DashboardView>();

			NavigationService.Register<RelativesViewModel, RelativesView>();
			NavigationService.Register<RelativeDetailsViewModel, RelativeView>();

			NavigationService.Register<StudentsViewModel, StudentsView>();
			NavigationService.Register<StudentDetailsViewModel, StudentView>();

			NavigationService.Register<StudentBelongingDetailsViewModel, StudentBelongingView>();
			NavigationService.Register<StudentRelativeDetailsViewModel, StudentRelativeView>();

			NavigationService.Register<ClassroomsViewModel, ClassroomsView>();
			NavigationService.Register<ClassroomDetailsViewModel, ClassroomView>();

			NavigationService.Register<ClassroomStudentDetailsViewModel, ClassroomStudentView>();

			NavigationService.Register<DismissibleStudentsViewModel, DismissibleStudentsView>();
			NavigationService.Register<DismissalsViewModel, DismissalsView>();
			NavigationService.Register<DismissalDetailsViewModel, DismissalView>();

			NavigationService.Register<LostAndFoundsViewModel, LostAndFoundsView>();
			NavigationService.Register<LostAndFoundDetailsViewModel, LostAndFoundView>();

			NavigationService.Register<CompaniesViewModel, CompaniesView>();
			NavigationService.Register<CompanyDetailsViewModel, CompanyView>();

			NavigationService.Register<CompanyUserDetailsViewModel, CompanyUserView>();

			NavigationService.Register<SubscriptionsViewModel, SubscriptionsView>();
			NavigationService.Register<SubscriptionDetailsViewModel, SubscriptionView>();

			NavigationService.Register<PermissionsViewModel, PermissionsView>();
			NavigationService.Register<PermissionDetailsViewModel, PermissionView>();

			NavigationService.Register<RolesViewModel, RolesView>();
			NavigationService.Register<RoleDetailsViewModel, RoleView>();

			NavigationService.Register<RolePermissionDetailsViewModel, RolePermissionView>();

			NavigationService.Register<UsersViewModel, UsersView>();
			NavigationService.Register<UserDetailsViewModel, UserView>();

			NavigationService.Register<UserRoleDetailsViewModel, UserRoleView>();

			NavigationService.Register<AppLogsViewModel, AppLogsView>();

			NavigationService.Register<SettingsViewModel, SettingsView>();
		}

		static private async Task ConfigureLookupTables()
		{
			var lookupTables = ServiceLocator.Current.GetService<ILookupTables>();
			await lookupTables.InitializeAsync();
			LookupTablesProxy.Instance = lookupTables;
		}
	}
}
