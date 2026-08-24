using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class RelativeModel : ObservableObject
	{
		public static RelativeModel CreateEmpty() => new() { RelativeID = -1, IsEmpty = true };
		public long RelativeID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public short RelativeTypeID { get; set; }
		public string DocumentNumber { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }

		public bool IsNew => RelativeID <= 0;
		public string FullName => $"{FirstName} {LastName}";
		public string Initials => String.Format("{0}{1}", $"{FirstName} "[0], $"{LastName} "[0]).Trim().ToUpper();

		public byte[] Picture { get; set; }
		public object PictureSource { get; set; }

		public byte[] Thumbnail { get; set; }
		public object ThumbnailSource { get; set; }

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public RelativeTypeModel RelativeType { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is RelativeModel model)
			{
				Merge(model);
			}
		}

		public void Merge(RelativeModel source)
		{
			if (source != null)
			{
				RelativeID = source.RelativeID;
				FirstName = source.FirstName;
				MiddleName = source.MiddleName;
				LastName = source.LastName;
				RelativeTypeID = source.RelativeTypeID;
				DocumentNumber = source.DocumentNumber;
				Phone = source.Phone;
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
