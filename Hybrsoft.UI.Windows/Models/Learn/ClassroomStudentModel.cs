using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class ClassroomStudentModel : ObservableObject
	{
		public long ClassroomStudentID { get; set; }

		public long ClassroomID { get; set; }
		public long StudentID { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public ClassroomModel Classroom { get; set; }
		public StudentModel Student { get; set; }

		public bool IsNew => ClassroomStudentID <= 0;

		public override void Merge(ObservableObject source)
		{
			if (source is ClassroomStudentModel model)
			{
				Merge(model);
			}
		}

		public void Merge(ClassroomStudentModel source)
		{
			if (source != null)
			{
				ClassroomStudentID = source.ClassroomStudentID;
				ClassroomID = source.ClassroomID;
				Classroom = source.Classroom;
				StudentID = source.StudentID;
				Student = source.Student;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
