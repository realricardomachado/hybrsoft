using Hybrsoft.Domain.Services;
using Hybrsoft.EnterpriseManager.Common;
using Hybrsoft.EnterpriseManager.Services;
using Hybrsoft.EnterpriseManager.Services.DataServiceFactory;
using Hybrsoft.EnterpriseManager.Services.Infrastructure;
using Hybrsoft.EnterpriseManager.Services.Infrastructure.LogService;
using Hybrsoft.UI.Windows.Infrastructure.Services;
using Hybrsoft.UI.Windows.Services;
using Hybrsoft.UI.Windows.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Hybrsoft.EnterpriseManager.Configuration
{
	public partial class ServiceLocator : IDisposable
	{
		static private readonly ConcurrentDictionary<ulong, ServiceLocator> _serviceLocators = new();

		private static ServiceProvider _rootServiceProvider = null;

		public static void Configure(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<ISettingsStore, SettingsStore>();
			serviceCollection.AddSingleton<ISettingsService, SettingsService>();
			serviceCollection.AddSingleton<IConnectionConfigurationService, ConnectionConfigurationService>();
			serviceCollection.AddSingleton<IDataServiceFactory, DataServiceFactory>();
			serviceCollection.AddSingleton<ILookupTables, LookupTables>();
			serviceCollection.AddSingleton<IRelativeService, RelativeService>();
			serviceCollection.AddSingleton<IStudentService, StudentService>();
			serviceCollection.AddSingleton<IStudentBelongingService, StudentBelongingService>();
			serviceCollection.AddSingleton<IStudentRelativeService, StudentRelativeService>();
			serviceCollection.AddSingleton<IClassroomService, ClassroomService>();
			serviceCollection.AddSingleton<IClassroomStudentService, ClassroomStudentService>();
			serviceCollection.AddSingleton<IDismissalService, DismissalService>();
			serviceCollection.AddSingleton<ILostAndFoundService, LostAndFoundService>();
			serviceCollection.AddSingleton<ICompanyService, CompanyService>();
			serviceCollection.AddSingleton<ICompanyUserService, CompanyUserService>();
			serviceCollection.AddSingleton<ISubscriptionService, SubscriptionService>();
			serviceCollection.AddSingleton<IPermissionService, PermissionService>();
			serviceCollection.AddSingleton<IRoleService, RoleService>();
			serviceCollection.AddSingleton<IRolePermissionService, RolePermissionService>();
			serviceCollection.AddSingleton<IUserService, UserService>();
			serviceCollection.AddSingleton<IUserRoleService, UserRoleService>();

			serviceCollection.AddSingleton<IDialogService, DialogService>();
			serviceCollection.AddSingleton<IFilePickerService, FilePickerService>();
			serviceCollection.AddSingleton<ILicenseService, LicenseService>();
			serviceCollection.AddSingleton<ILoginService, LoginService>();
			serviceCollection.AddSingleton<ILogService, LogService>();
			serviceCollection.AddSingleton<IMessageService, MessageService>();
			serviceCollection.AddSingleton<INetworkService, NetworkService>();
			serviceCollection.AddSingleton<IResourceService, ResourceService>();
			serviceCollection.AddSingleton<ISecurityService, SecurityService>();
			serviceCollection.AddSingleton<IWindowsSecurityService, WindowsSecurityService>();
			serviceCollection.AddSingleton<IAuthorizationService, AuthorizationService>();
			serviceCollection.AddSingleton<IEmbeddingService>(sp =>
				EmbeddingServiceFactory.CreateAzure(
					modelName: AppConfig.Azure_OpenAI_Embedding_Model,
					apiKey: AppConfig.Azure_OpenAI_API_Key,
					endpoint: new Uri(AppConfig.Azure_OpenAI_API_Endpoint)
				)
			);
			serviceCollection.AddSingleton<IEmbeddingTables, LogEmbeddingService>();
			serviceCollection.AddSingleton<IEmbeddingTables, LostAndFoundEmbeddingService>();
			serviceCollection.AddSingleton<IEmbeddingTables, StudentBelongingEmbeddingService>();

			serviceCollection.AddScoped<IContextService, ContextService>();
			serviceCollection.AddScoped<INavigationService, NavigationService>();
			serviceCollection.AddScoped<ICommonServices, CommonServices>();
			serviceCollection.AddScoped<ITitleService, TitleService>();

			serviceCollection.AddTransient<LoginViewModel>();
			serviceCollection.AddTransient<LicenseActivationViewModel>();

			serviceCollection.AddTransient<ShellViewModel>();
			serviceCollection.AddTransient<MainShellViewModel>();

			serviceCollection.AddTransient<DashboardViewModel>();

			serviceCollection.AddTransient<RelativesViewModel>();
			serviceCollection.AddTransient<RelativeDetailsViewModel>();

			serviceCollection.AddTransient<StudentsViewModel>();
			serviceCollection.AddTransient<StudentDetailsViewModel>();
			serviceCollection.AddTransient<StudentMasterDetailsViewModel>();

			serviceCollection.AddTransient<StudentBelongingDetailsViewModel>();
			serviceCollection.AddTransient<StudentRelativeDetailsViewModel>();

			serviceCollection.AddTransient<ClassroomsViewModel>();
			serviceCollection.AddTransient<ClassroomDetailsViewModel>();
			serviceCollection.AddTransient<ClassroomMasterDetailsViewModel>();

			serviceCollection.AddTransient<ClassroomStudentDetailsViewModel>();

			serviceCollection.AddTransient<DismissibleStudentsViewModel>();
			serviceCollection.AddTransient<DismissalsViewModel>();
			serviceCollection.AddTransient<DismissalDetailsViewModel>();

			serviceCollection.AddTransient<LostAndFoundsViewModel>();
			serviceCollection.AddTransient<LostAndFoundDetailsViewModel>();

			serviceCollection.AddTransient<CompaniesViewModel>();
			serviceCollection.AddTransient<CompanyDetailsViewModel>();
			serviceCollection.AddTransient<CompanyMasterDetailsViewModel>();

			serviceCollection.AddTransient<CompanyUserDetailsViewModel>();

			serviceCollection.AddTransient<SubscriptionsViewModel>();
			serviceCollection.AddTransient<SubscriptionDetailsViewModel>();

			serviceCollection.AddTransient<PermissionsViewModel>();
			serviceCollection.AddTransient<PermissionDetailsViewModel>();

			serviceCollection.AddTransient<RolesViewModel>();
			serviceCollection.AddTransient<RoleDetailsViewModel>();
			serviceCollection.AddTransient<RoleMasterDetailsViewModel>();

			serviceCollection.AddTransient<RolePermissionDetailsViewModel>();

			serviceCollection.AddTransient<UsersViewModel>();
			serviceCollection.AddTransient<UserDetailsViewModel>();
			serviceCollection.AddTransient<UserMasterDetailsViewModel>();

			serviceCollection.AddTransient<UserRoleDetailsViewModel>();

			serviceCollection.AddTransient<AppLogsViewModel>();

			serviceCollection.AddTransient<SettingsViewModel>();

			_rootServiceProvider = serviceCollection.BuildServiceProvider();
		}

		static public ServiceLocator Current
		{
			get
			{
				ulong currentViewId = WindowTracker.GetCurrentView().ID;
				return _serviceLocators.GetOrAdd(currentViewId, key => new ServiceLocator());
			}
		}

		static public void DisposeCurrent()
		{
			ulong currentViewId = WindowTracker.GetCurrentView().ID;
			if (_serviceLocators.TryRemove(currentViewId, out ServiceLocator current))
			{
				current.Dispose();
			}
		}
		static public void DisposeByViewID(ulong viewID)
		{
			if (_serviceLocators.TryRemove(viewID, out ServiceLocator service))
			{
				service.Dispose();
			}
		}

		private readonly IServiceScope _serviceScope = null;

		private ServiceLocator()
		{
			_serviceScope = _rootServiceProvider.CreateScope();
		}

		public T GetService<T>()
		{
			return GetService<T>(true);
		}

		public T GetService<T>(bool isRequired)
		{
			return isRequired
				? _serviceScope.ServiceProvider.GetRequiredService<T>()
				: _serviceScope.ServiceProvider.GetService<T>();
		}

		#region Dispose
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_serviceScope?.Dispose();
			}
		}
		#endregion
	}
}
