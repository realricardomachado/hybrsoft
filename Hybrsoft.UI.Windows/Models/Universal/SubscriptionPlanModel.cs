using Hybrsoft.UI.Windows.Infrastructure.ViewModels;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class SubscriptionPlanModel : ObservableObject
	{
		public short SubscriptionPlanID { get; set; }
		public string Name { get; set; }
		public short DurationMonths { get; set; }
	}
}
