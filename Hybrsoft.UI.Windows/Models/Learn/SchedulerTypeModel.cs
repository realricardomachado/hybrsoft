using Hybrsoft.UI.Windows.Infrastructure.ViewModels;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class ScheduleTypeModel : ObservableObject
	{
		public short ScheduleTypeID { get; set; }
		public string Name { get; set; }
	}
}
