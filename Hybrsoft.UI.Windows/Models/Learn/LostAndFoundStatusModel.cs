using Hybrsoft.UI.Windows.Infrastructure.ViewModels;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class LostAndFoundStatusModel : ObservableObject
	{
		public short LostAndFoundStatusID { get; set; }
		public string DisplayName { get; set; }
	}
}
