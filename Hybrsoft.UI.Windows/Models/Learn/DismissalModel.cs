using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class DismissalModel : ObservableObject
	{
		public static DismissalModel CreateEmpty() => new() { DismissalID = -1, IsEmpty = true };
		public long DismissalID { get; set; }
		public long ClassroomID { get; set; }
		public long StudentID { get; set; }
		public long RelativeID { get; set; }
		public bool IsNew => DismissalID <= 0;
		public bool IsDismissed => DismissedOn.HasValue;

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? DismissedOn { get; set; }

		public virtual ClassroomModel Classroom { get; set; }
		public virtual StudentModel Student { get; set; }
		public virtual RelativeModel Relative { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is DismissalModel model)
			{
				Merge(model);
			}
		}

		public void Merge(DismissalModel source)
		{
			if (source != null)
			{
				DismissalID = source.DismissalID;
				ClassroomID = source.ClassroomID;
				Classroom = source.Classroom;
				StudentID = source.StudentID;
				Student = source.Student;
				RelativeID = source.RelativeID;
				Relative = source.Relative;
				CreatedOn = source.CreatedOn;
				DismissedOn = source.DismissedOn;
			}
		}
	}
}
