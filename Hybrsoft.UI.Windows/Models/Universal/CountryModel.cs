using Hybrsoft.UI.Windows.Infrastructure.ViewModels;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class CountryModel : ObservableObject
	{
		public short CountryID { get; set; }
		public string Name { get; set; }
	}
}
