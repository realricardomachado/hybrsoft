using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class StudentBelongingModel : ObservableObject
	{
		public long StudentBelongingID { get; set; }
		public long StudentID { get; set; }

		public string DisplayName { get; set; }
		public string Description { get; set; }

		public bool IsNew => StudentBelongingID <= 0;

		public byte[] Picture { get; set; }
		public object PictureSource { get; set; }

		public byte[] Thumbnail { get; set; }
		public object ThumbnailSource { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public virtual StudentModel Student { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is StudentBelongingModel model)
			{
				Merge(model);
			}
		}

		public void Merge(StudentBelongingModel source)
		{
			if (source != null)
			{
				StudentBelongingID = source.StudentBelongingID;
				StudentID = source.StudentID;
				Student = source.Student;
				DisplayName = source.DisplayName;
				Description = source.Description;
				Picture = source.Picture;
				PictureSource = source.PictureSource;
				Thumbnail = source.Thumbnail;
				ThumbnailSource = source.ThumbnailSource;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
