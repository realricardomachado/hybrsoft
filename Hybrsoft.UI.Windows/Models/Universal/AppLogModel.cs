using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Hybrsoft.Enums;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class AppLogModel : ObservableObject
	{
		public static AppLogModel CreateEmpty() => new() { AppLogID = -1, IsEmpty = true };

		public long AppLogID { get; set; }
		public bool IsRead { get; set; }
		public DateTimeOffset CreatedOn { get; set; }
		public string User { get; set; }
		public LogType Type { get; set; }
		public string Source { get; set; }
		public string Action { get; set; }
		public string Message { get; set; }
		public string Description { get; set; }
		public AppType AppType { get; set; }
	}
}
