using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class CompanyUserModel : ObservableObject
	{
		public long CompanyUserID { get; set; }

		public long CompanyID { get; set; }
		public long UserID { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public CompanyModel Company { get; set; }
		public UserModel User { get; set; }

		public bool IsNew => CompanyUserID <= 0;

		public override void Merge(ObservableObject source)
		{
			if (source is CompanyUserModel model)
			{
				Merge(model);
			}
		}

		public void Merge(CompanyUserModel source)
		{
			if (source != null)
			{
				CompanyUserID = source.CompanyUserID;
				CompanyID = source.CompanyID;
				UserID = source.UserID;
				User = source.User;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
