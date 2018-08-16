using Epsiloner.Wpf.Behaviors;

namespace Epsiloner.Wpf
{
    /// <summary>
    /// Item which can be selected by <see cref="KeyboardNavigationBehavior"/>.
    /// </summary>
    public interface ISelectableItem
    {
        /// <summary>
        /// Indicates that this item is currenty selected.
        /// </summary>
        bool IsSelected { get; set; }
    }
}
