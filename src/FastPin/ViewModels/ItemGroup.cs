using System;
using System.Collections.ObjectModel;

namespace FastPin.ViewModels
{
    /// <summary>
    /// Represents a group of items by date
    /// </summary>
    public class ItemGroup
    {
        public string DateGroup { get; set; } = string.Empty;
        public ObservableCollection<PinnedItemViewModel> Items { get; set; } = new ObservableCollection<PinnedItemViewModel>();
    }
}
