using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Hybrsoft.UI.Windows.Services;
using Hybrsoft.Enums;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class SubscriptionModel : ObservableObject
	{
		public static SubscriptionModel CreateEmpty() => new() { SubscriptionID = -1, IsEmpty = true };
		public long SubscriptionID { get; set; }
		public short SubscriptionPlanID { get; set; }
		public short DurationDays { get; set; }
		public DateTimeOffset? StartDate { get; set; }
		public DateTimeOffset? ExpirationDate { get; set; }
		public SubscriptionType Type { get; set; }
		public long CompanyID { get; set; }
		public long UserID { get; set; }
		public string LicenseKey { get; set; }
		public SubscriptionStatus Status { get; set; }
		public DateTimeOffset? CancelledOn { get; set; }
		public DateTimeOffset? LastValidatedOn { get; set; }

		public bool IsNew => SubscriptionID <= 0;
		public string FullName => $"{LicenseKey} - {LicencedTo}";

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public string LicencedTo => Type == Enums.SubscriptionType.Enterprise ? Company.FullName : User.FullName;
		public string StatusDisplayName => LookupTablesProxy.Instance?.GetSubscriptionStatus((short)Status);
		public virtual SubscriptionTypeModel SubscriptionType { get; set; }
		public virtual CompanyModel Company { get; set; }
		public virtual UserModel User { get; set; }

		public bool CanEditUser => Type == Enums.SubscriptionType.Individual;
		public bool CanEditCompany => Type == Enums.SubscriptionType.Enterprise;
		public bool IsCancelled => CancelledOn.HasValue;

		public override void Merge(ObservableObject source)
		{
			if (source is SubscriptionModel model)
			{
				Merge(model);
			}
		}
		public void Merge(SubscriptionModel source)
		{
			if (source != null)
			{
				SubscriptionID = source.SubscriptionID;
				SubscriptionPlanID = source.SubscriptionPlanID;
				DurationDays = source.DurationDays;
				StartDate = source.StartDate;
				ExpirationDate = source.ExpirationDate;
				CompanyID = source.CompanyID;
				Company = source.Company;
				UserID = source.UserID;
				User = source.User;
				LicenseKey = source.LicenseKey;
				Status = source.Status;
				CancelledOn = source.CancelledOn;
				LastValidatedOn = source.LastValidatedOn;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
