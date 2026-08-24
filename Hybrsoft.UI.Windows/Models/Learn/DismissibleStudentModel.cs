using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class DismissibleStudentModel : ObservableObject
	{
		public static DismissibleStudentModel CreateEmpty() => new() { IsEmpty = true };

		public long ClassroomID { get; set; }
		public string ClassroomName { get; set; }
		public long StudentID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }

		public string FullName => $"{FirstName} {LastName}";
		public string Initials => String.Format("{0}{1}", $"{FirstName} "[0], $"{LastName} "[0]).Trim().ToUpper();

		public object ThumbnailSource { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is DismissibleStudentModel model)
			{
				Merge(model);
			}
		}

		public void Merge(DismissibleStudentModel source)
		{
			if (source != null)
			{
				ClassroomID = source.ClassroomID;
				ClassroomName = source.ClassroomName;
				StudentID = source.StudentID;
				FirstName = source.FirstName;
				MiddleName = source.MiddleName;
				LastName = source.LastName;
				ThumbnailSource = source.ThumbnailSource;
			}
		}
	}
}
