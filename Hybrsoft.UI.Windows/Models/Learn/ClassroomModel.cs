using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class ClassroomModel : ObservableObject
	{
		public static ClassroomModel CreateEmpty() => new() { ClassroomID = -1, IsEmpty = true };
		public long ClassroomID { get; set; }
		public string Name { get; set; }
		public short Year { get; set; }
		public short MinimumYear { get; set; }
		public short MaximumYear { get; set; }
		public short EducationLevel { get; set; }
		public short MinimumEducationLevel { get; set; }
		public short MaximumEducationLevel { get; set; }
		public short ScheduleTypeID { get; set; }
		public bool IsNew => ClassroomID <= 0;

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public ScheduleTypeModel ScheduleType { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is ClassroomModel model)
			{
				Merge(model);
			}
		}

		public void Merge(ClassroomModel source)
		{
			if (source != null)
			{
				ClassroomID = source.ClassroomID;
				Name = source.Name;
				Year = source.Year;
				MinimumYear = source.MinimumYear;
				MaximumYear = source.MaximumYear;
				EducationLevel = source.EducationLevel;
				MinimumEducationLevel = source.MinimumEducationLevel;
				MaximumEducationLevel = source.MaximumEducationLevel;
				ScheduleTypeID = source.ScheduleTypeID;
				ScheduleType = source.ScheduleType;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
