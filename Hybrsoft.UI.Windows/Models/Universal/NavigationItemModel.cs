using Hybrsoft.UI.Windows.Infrastructure.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;

namespace Hybrsoft.UI.Windows.Models
{
	public partial class NavigationItemModel(Type viewModel) : ObservableObject
	{
		public NavigationItemModel(string label,
			int glyph,
			string tag,
			int? parentID,
			ObservableCollection<NavigationItemModel> children,
			Type viewModel) : this(viewModel)
		{
			Label = label;
			Icon = new SymbolIcon((Symbol)glyph);
			Tag = tag;
			ParentID = parentID;
			Children = children;
			ViewModel = viewModel;
		}

		public readonly string Label;
		public readonly SymbolIcon Icon;
		public readonly string Tag;
		public readonly int? ParentID;
		public ObservableCollection<NavigationItemModel> Children;
		public readonly Type ViewModel = viewModel;

		private InfoBadge _badge = null;
		public InfoBadge Badge
		{
			get => _badge;
			set => Set(ref _badge, value);
		}
	}
}
