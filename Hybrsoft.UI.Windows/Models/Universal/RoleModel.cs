using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class RoleModel : ObservableObject
	{
		public static RoleModel CreateEmpty() => new() { RoleID = -1, IsEmpty = true };

		public long RoleID { get; set; }

		public string Name { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public bool IsNew => RoleID <= 0;

		public override void Merge(ObservableObject source)
		{
			if (source is RoleModel model)
			{
				Merge(model);
			}
		}

		public void Merge(RoleModel source)
		{
			if (source != null)
			{
				RoleID = source.RoleID;
				Name = source.Name;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
