using Hybrsoft.Enums;
using Hybrsoft.UI.Windows.Services;
using Hybrsoft.UI.Windows.ViewModels;
using System;
using System.Linq;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	public class AuthorizationService(ILookupTables lookupTables) : IAuthorizationService
	{
		private readonly ILookupTables _lookupTables = lookupTables;
		public bool HasSecurityAdministrationPermission => HasPermission(Permissions.SecurityAdministration);

		public bool HasPermission(Permissions permission)
		{
			return _lookupTables.Permissions?.Any(r => r.Name == permission.ToString()) == true;
		}

		public bool CanAccessViewModel(Type viewModel)
		{
			bool hasSecurityAdministration = HasSecurityAdministrationPermission;

			if (viewModel == typeof(DashboardViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.DashboardReader);
			}
			if (viewModel == typeof(RelativesViewModel)
				|| viewModel == typeof(RelativeDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.RelativeReader);
			}
			if (viewModel == typeof(StudentsViewModel)
				|| viewModel == typeof(StudentDetailsViewModel)
				|| viewModel == typeof(StudentBelongingDetailsViewModel)
				|| viewModel == typeof(StudentRelativeDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.StudentReader);
			}
			if (viewModel == typeof(ClassroomsViewModel)
				|| viewModel == typeof(ClassroomDetailsViewModel)
				|| viewModel == typeof(ClassroomStudentDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.ClassroomReader);
			}
			if (viewModel == typeof(DismissibleStudentsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.DismissibleStudentsReader);
			}
			if (viewModel == typeof(DismissalsViewModel)
				|| viewModel == typeof(DismissalDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.DismissalReader);
			}
			if (viewModel == typeof(LostAndFoundsViewModel)
				|| viewModel == typeof(LostAndFoundDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.LostAndFoundReader);
			}
			if (viewModel == typeof(CompaniesViewModel)
				|| viewModel == typeof(CompanyDetailsViewModel)
				|| viewModel == typeof(CompanyUserDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.CompanyReader);
			}
			if (viewModel == typeof(SubscriptionsViewModel)
				|| viewModel == typeof(SubscriptionDetailsViewModel))
			{
				return hasSecurityAdministration || HasPermission(Permissions.SubscriptionReader);
			}

			return (viewModel == typeof(PermissionsViewModel)
				|| viewModel == typeof(PermissionDetailsViewModel)
				|| viewModel == typeof(RolesViewModel)
				|| viewModel == typeof(RoleDetailsViewModel)
				|| viewModel == typeof(RolePermissionDetailsViewModel)
				|| viewModel == typeof(UsersViewModel)
				|| viewModel == typeof(UserDetailsViewModel)
				|| viewModel == typeof(UserRoleDetailsViewModel)
				|| viewModel == typeof(AppLogsViewModel)
				|| viewModel == typeof(AppLogDetailsViewModel))
				&& hasSecurityAdministration;
		}

		public Type GetDefaultAccessibleViewModel()
		{
			Type[] viewModels =
			[
				typeof(DashboardViewModel),
				typeof(RelativesViewModel),
				typeof(StudentsViewModel),
				typeof(ClassroomsViewModel),
				typeof(DismissibleStudentsViewModel),
				typeof(DismissalsViewModel),
				typeof(LostAndFoundsViewModel),
				typeof(CompaniesViewModel),
				typeof(SubscriptionsViewModel),
				typeof(PermissionsViewModel),
				typeof(RolesViewModel),
				typeof(UsersViewModel),
				typeof(AppLogsViewModel)
			];

			return viewModels.FirstOrDefault(CanAccessViewModel);
		}
	}
}
