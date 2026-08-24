using Hybrsoft.UI.Windows.Infrastructure.ViewModels;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class SubscriptionStatusModel : ObservableObject
	{
		public short SubscriptionStatusID { get; set; }
		public string DisplayName { get; set; }
	}
}
