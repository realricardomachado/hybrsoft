using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class StudentModel : ObservableObject
	{
		public static StudentModel CreateEmpty() => new() { StudentID = -1, IsEmpty = true };
		public long StudentID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }

		public bool IsNew => StudentID <= 0;
		public string FullName => $"{FirstName} {LastName}";
		public string Initials => String.Format("{0}{1}", $"{FirstName} "[0], $"{LastName} "[0]).Trim().ToUpper();

		public byte[] Picture { get; set; }
		public object PictureSource {  get; set; }

		public byte[] Thumbnail { get; set; }
		public object ThumbnailSource { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is StudentModel model)
			{
				Merge(model);
			}
		}

		public void Merge(StudentModel source)
		{
			if (source != null)
			{
				StudentID = source.StudentID;
				FirstName = source.FirstName;
				MiddleName = source.MiddleName;
				LastName = source.LastName;
				Email = source.Email;
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
