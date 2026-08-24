using Hybrsoft.UI.Windows.Infrastructure.ViewModels;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class SubscriptionTypeModel : ObservableObject
	{
		public short SubscriptionTypeID { get; set; }
		public string DisplayName { get; set; }
	}
}
