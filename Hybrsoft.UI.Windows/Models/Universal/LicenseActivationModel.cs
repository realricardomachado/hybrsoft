using Hybrsoft.Enums;

namespace Hybrsoft.UI.Windows.Models
{
	public class LicenseActivationModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string LicenseKey { get; set; }
		public AppType ProductType { get; set; }
	}
}
