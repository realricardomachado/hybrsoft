using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Hybrsoft.UI.Windows.ViewModels;
using Hybrsoft.EnterpriseManager.Extensions;
using System;
using Windows.ApplicationModel.Activation;

namespace Hybrsoft.EnterpriseManager.Services.Infrastructure
{
	#region ActivationInfo
	public class ActivationInfo
	{
		static public ActivationInfo CreateDefault() => Create<DashboardViewModel>();

		static public ActivationInfo Create<TViewModel>(object entryArgs = null) where TViewModel : ViewModelBase
		{
			return new ActivationInfo
			{
				EntryViewModel = typeof(TViewModel),
				EntryArgs = entryArgs
			};
		}

		public Type EntryViewModel { get; set; }
		public object EntryArgs { get; set; }
	}
	#endregion

	static public class ActivationService
	{
		public static ActivationInfo GetActivationInfo(IActivatedEventArgs args)
		{
			return args.Kind switch
			{
				ActivationKind.Protocol => GetProtocolActivationInfo(args as ProtocolActivatedEventArgs),
				_ => ActivationInfo.CreateDefault()
			};
		}

		private static ActivationInfo GetProtocolActivationInfo(ProtocolActivatedEventArgs args)
		{
			if (args != null)
			{
				switch (args.Uri.Host.ToLowerInvariant())
				{
					case "relative":
					case "relatives":
						long relativeID = args.Uri.GetInt64Parameter("id");
						if (relativeID > 0)
						{
							return ActivationInfo.Create<RelativeDetailsViewModel>(new RelativeDetailsArgs { RelativeID = relativeID });
						}
						return ActivationInfo.Create<RelativesViewModel>(new RelativeListArgs());
					case "student":
					case "students":
						long studentID = args.Uri.GetInt64Parameter("id");
						if (studentID > 0)
						{
							return ActivationInfo.Create<StudentDetailsViewModel>(new StudentDetailsArgs { StudentID = studentID });
						}
						return ActivationInfo.Create<StudentsViewModel>(new StudentListArgs());
					case "classroom":
					case "classrooms":
						long classroomID = args.Uri.GetInt64Parameter("id");
						if (classroomID > 0)
						{
							return ActivationInfo.Create<ClassroomDetailsViewModel>(new ClassroomDetailsArgs { ClassroomID = classroomID });
						}
						return ActivationInfo.Create<ClassroomsViewModel>(new ClassroomListArgs());
					case "dismissiblestudents":
						return ActivationInfo.Create<DismissibleStudentsViewModel>(new DismissibleStudentListArgs());
					case "dismissals":
						return ActivationInfo.Create<DismissalsViewModel>(new DismissalListArgs());
					case "lostandfounds":
						long lostAndFoundID = args.Uri.GetInt64Parameter("id");
						if (lostAndFoundID > 0)
						{
							return ActivationInfo.Create<LostAndFoundDetailsViewModel>(new LostAndFoundDetailsArgs { LostAndFoundID = lostAndFoundID });
						}
						return ActivationInfo.Create<LostAndFoundsViewModel>(new LostAndFoundListArgs());
					case "company":
					case "companies":
						long companyID = args.Uri.GetInt64Parameter("id");
						if (companyID > 0)
						{
							return ActivationInfo.Create<CompanyDetailsViewModel>(new CompanyDetailsArgs { CompanyID = companyID });
						}
						return ActivationInfo.Create<CompaniesViewModel>(new CompanyListArgs());
					case "subscription":
					case "subscriptions":
						long subscriptionID = args.Uri.GetInt64Parameter("id");
						if (subscriptionID > 0)
						{
							return ActivationInfo.Create<SubscriptionDetailsViewModel>(new SubscriptionDetailsArgs { SubscriptionID = subscriptionID });
						}
						return ActivationInfo.Create<SubscriptionsViewModel>(new SubscriptionListArgs());
					case "permission":
					case "permissions":
						long permissionID = args.Uri.GetInt64Parameter("id");
						if (permissionID > 0)
						{
							return ActivationInfo.Create<PermissionDetailsViewModel>(new PermissionDetailsArgs { PermissionID = permissionID });
						}
						return ActivationInfo.Create<PermissionsViewModel>(new PermissionListArgs());
					case "role":
					case "roles":
						long roleID = args.Uri.GetInt64Parameter("id");
						if (roleID > 0)
						{
							return ActivationInfo.Create<RoleDetailsViewModel>(new RoleDetailsArgs { RoleID = roleID });
						}
						return ActivationInfo.Create<RolesViewModel>(new RoleListArgs());
					case "user":
					case "users":
						long userID = args.Uri.GetInt64Parameter("id");
						if (userID > 0)
						{
							return ActivationInfo.Create<UserDetailsViewModel>(new UserDetailsArgs { UserID = userID });
						}
						return ActivationInfo.Create<UsersViewModel>(new UserListArgs());
				}
			}
			return ActivationInfo.CreateDefault();
		}
	}
}
