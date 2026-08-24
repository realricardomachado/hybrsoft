using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class CompanyModel : ObservableObject
	{
		public static CompanyModel CreateEmpty() => new() { CompanyID = -1, IsEmpty = true };
		public long CompanyID { get; set; }
		public string LegalName { get; set; }
		public string TradeName { get; set; }
		public string FederalRegistration { get; set; }
		public string StateRegistration { get; set; }
		public string CityLicense { get; set; }
		public short CountryID { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }

		public bool IsNew => CompanyID <= 0;
		public string FullName => $"{LegalName} ({TradeName})";

		public DateTimeOffset CreatedOn { get; set; }
		public DateTimeOffset? LastModifiedOn { get; set; }

		public CountryModel Country { get; set; }

		public override void Merge(ObservableObject source)
		{
			if (source is CompanyModel model)
			{
				Merge(model);
			}
		}

		public void Merge(CompanyModel source)
		{
			if (source != null)
			{
				CompanyID = source.CompanyID;
				LegalName = source.LegalName;
				TradeName = source.TradeName;
				FederalRegistration = source.FederalRegistration;
				StateRegistration = source.StateRegistration;
				CityLicense = source.CityLicense;
				CountryID = source.CountryID;
				Phone = source.Phone;
				Email = source.Email;
				CreatedOn = source.CreatedOn;
				LastModifiedOn = source.LastModifiedOn;
			}
		}
	}
}
