using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class StudentRelativeModel : ObservableObject
	{
		public long StudentRelativeID { get; set; }

		public long StudentID { get; set; }
		public long RelativeID { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public RelativeModel Relative { get; set; }

		public bool IsNew => StudentRelativeID <= 0;

		public override void Merge(ObservableObject source)
		{
			if (source is StudentRelativeModel model)
			{
				Merge(model);
			}
		}

		public void Merge(StudentRelativeModel source)
		{
			if (source != null)
			{
				StudentRelativeID = source.StudentRelativeID;
				StudentID = source.StudentID;
				RelativeID = source.RelativeID;
				Relative = source.Relative;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
