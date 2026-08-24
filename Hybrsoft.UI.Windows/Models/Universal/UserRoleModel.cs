using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class UserRoleModel : ObservableObject
	{
		public long UserRoleID { get; set; }

		public long UserID { get; set; }
		public long RoleID { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public RoleModel Role { get; set; }

		public bool IsNew => UserRoleID <= 0;

		public override void Merge(ObservableObject source)
		{
			if (source is UserRoleModel model)
			{
				Merge(model);
			}
		}

		public void Merge(UserRoleModel source)
		{
			if (source != null)
			{
				UserRoleID = source.UserRoleID;
				UserID = source.UserID;
				RoleID = source.RoleID;
				Role = source.Role;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
