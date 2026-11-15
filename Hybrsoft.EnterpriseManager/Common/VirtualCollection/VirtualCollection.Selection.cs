using Hybrsoft.EnterpriseManager.Extensions;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;

namespace Hybrsoft.EnterpriseManager.Common.VirtualCollection
{
	partial class VirtualCollection<T> : ISelectionInfo
	{
		private IList<ItemIndexRange> _rangeSelection = [];

		public void DeselectRange(ItemIndexRange itemIndexRange)
		{
			_rangeSelection = _rangeSelection.Subtract(itemIndexRange);
		}

		public IReadOnlyList<ItemIndexRange> GetSelectedRanges()
		{
			return (ItemIndexRange[])[.. _rangeSelection];
		}

		public bool IsSelected(int index)
		{
			foreach (ItemIndexRange range in _rangeSelection)
			{
				if (index >= range.FirstIndex && index <= range.LastIndex) return true;
			}
			return false;
		}

		public void SelectRange(ItemIndexRange itemIndexRange)
		{
			_rangeSelection = _rangeSelection.Merge(itemIndexRange);
		}
	}
}
