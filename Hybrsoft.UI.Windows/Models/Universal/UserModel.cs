using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class UserModel : ObservableObject
	{
		public static UserModel CreateEmpty() => new() { UserID = -1, IsEmpty = true };
		public long UserID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public bool PasswordChanged { get; set; }
		public int PasswordLength { get; set; }

		public bool IsNew => UserID <= 0;
		public string FullName => $"{FirstName} {LastName}";

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is UserModel model)
			{
				Merge(model);
			}
		}

		public void Merge(UserModel source)
		{
			if (source != null)
			{
				UserID = source.UserID;
				FirstName = source.FirstName;
				MiddleName = source.MiddleName;
				LastName = source.LastName;
				Email = source.Email;
				Password = source.Password;
				PasswordChanged = source.PasswordChanged;
				PasswordLength = source.PasswordLength;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
