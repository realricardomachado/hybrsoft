using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using System;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class LanguageModel : ObservableObject
	{
		public string DisplayName { get; set; }
		public string Tag { get; set; }

		#region Fix object bind SelectedItem in ComboBox
		public override bool Equals(object obj)
		{
			if (obj is LanguageModel other)
			{
				return Tag == other.Tag && DisplayName == other.DisplayName;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(DisplayName, Tag);
		}
		#endregion
	}
}
