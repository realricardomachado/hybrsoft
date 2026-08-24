using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class RolePermissionModel : ObservableObject
	{
		public long RolePermissionID { get; set; }

		public long RoleID { get; set; }
		public long PermissionID { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public PermissionModel Permission { get; set; }

		public bool IsNew => RolePermissionID <= 0;

		public override void Merge(ObservableObject source)
		{
			if (source is RolePermissionModel model)
			{
				Merge(model);
			}
		}

		public void Merge(RolePermissionModel source)
		{
			if (source != null)
			{
				RolePermissionID = source.RolePermissionID;
				RoleID = source.RoleID;
				PermissionID = source.PermissionID;
				Permission = source.Permission;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
