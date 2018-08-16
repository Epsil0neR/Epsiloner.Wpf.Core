using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Epsiloner.Wpf.Behaviors
{
    /// <summary>
    /// Provides navigation in specified items with ability to activate selected item.
    /// - By default <see cref="Key.Up"/> selects previous item.
    /// - <see cref="Key.Down"/> selects next item.
    /// - <see cref="Key.Enter"/> executes <see cref="ActivateSelectedCommand"/> for currently selected item (<see cref="Selected"/>).
    /// If selectable item implements <see cref="ISelectableItem"/>, then also sets <see cref="ISelectableItem.IsSelected"/> property when selects item.
    /// </summary>
    public class KeyboardNavigationBehavior : Behavior<UIElement>
    {
        /// <summary>
        /// Items where navigation can occur.
        /// </summary>
        public static DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items), typeof(IEnumerable<object>), typeof(KeyboardNavigationBehavior));

        /// <summary>
        /// Command to execute when <see cref="SelectedProperty"/> is set and pressing <see cref="Key.Enter"/>.
        /// </summary>
        public static DependencyProperty ActivateSelectedCommandProperty = DependencyProperty.Register(nameof(ActivateSelectedCommand), typeof(ICommand), typeof(KeyboardNavigationBehavior));

        /// <summary>
        /// Selected item from <see cref="ItemsProperty"/> list.
        /// </summary>
        public static DependencyProperty SelectedProperty = DependencyProperty.Register(nameof(Selected), typeof(object), typeof(KeyboardNavigationBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SeletedPropertyChangedCallback));

        /// <summary>
        /// Indicates if navigation between items is enabled.
        /// </summary>
        public static DependencyProperty IsEnabledProperty = DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(KeyboardNavigationBehavior),
            new PropertyMetadata(true));

        /// <summary>
        /// Key to select previous item. Default is <see cref="Key.Up"/>
        /// </summary>
        public static DependencyProperty PrevItemKeyProperty = DependencyProperty.Register(nameof(PrevItemKey), typeof(Key), typeof(KeyboardNavigationBehavior), new PropertyMetadata(Key.Up));

        /// <summary>
        /// Key to select next item. Default is <see cref="Key.Down"/>
        /// </summary>
        public static DependencyProperty NextItemKeyProperty = DependencyProperty.Register(nameof(NextItemKey), typeof(Key), typeof(KeyboardNavigationBehavior), new PropertyMetadata(Key.Down));

        /// <summary>
        /// Key to execute <see cref="ActivateSelectedCommandProperty"/> for <see cref="SelectedProperty"/>. Default is <see cref="Key.Enter"/>
        /// </summary>
        public static DependencyProperty ActivateSelectedItemKeyProperty = DependencyProperty.Register(nameof(ActivateSelectedItemKey), typeof(Key), typeof(KeyboardNavigationBehavior), new PropertyMetadata(Key.Enter));

        private static void SeletedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //In case old value implements ISelectableItem - set IsSelected to false
            var o = e.OldValue as ISelectableItem;
            if (o != null)
                o.IsSelected = false;

            //In case new value implements ISelectableItem - set IsSelected to true
            var n = e.NewValue as ISelectableItem;
            if (n != null)
                n.IsSelected = true;
        }

        /// <summary>
        /// (Dependency property) Command to execute when <see cref="Selected"/> is set and pressing <see cref="Key.Enter"/>.
        /// </summary>
        public ICommand ActivateSelectedCommand
        {
            get { return (ICommand)GetValue(ActivateSelectedCommandProperty); }
            set { SetValue(ActivateSelectedCommandProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Items where navigation can occur.
        /// </summary>
        public IEnumerable<object> Items
        {
            get { return (IEnumerable<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Selected item from <see cref="Items"/> list.
        /// </summary>
        public object Selected
        {
            get { return GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Indicates if navigation between items is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Key to select previous item. Default is <see cref="Key.Up"/>
        /// </summary>
        public Key PrevItemKey
        {
            get { return (Key)GetValue(PrevItemKeyProperty); }
            set { SetValue(PrevItemKeyProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Key to select next item. Default is <see cref="Key.Down"/>
        /// </summary>
        public Key NextItemKey
        {
            get { return (Key)GetValue(NextItemKeyProperty); }
            set { SetValue(NextItemKeyProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Key to execute <see cref="ActivateSelectedCommand"/> for <see cref="Selected"/>. Default is <see cref="Key.Enter"/>
        /// </summary>
        public Key ActivateSelectedItemKey
        {
            get { return (Key)GetValue(ActivateSelectedItemKeyProperty); }
            set { SetValue(ActivateSelectedItemKeyProperty, value); }
        }


        /// <inheritdoc />
        protected override void OnAttached()
        {
            AssociatedObject.PreviewKeyDown += AssociatedObjectOnPreviewKeyDown;
            base.OnAttached();
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObjectOnPreviewKeyDown;
            base.OnDetaching();
        }


        private void AssociatedObjectOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled || !IsEnabled || Items?.Any() != true)
                return;

            var selected = Selected;
            int ind;
            if (e.Key == PrevItemKey)
            {
                e.Handled = true;
                ind = GetIndexOfItem(selected);
                MoveSelectionToIndex(ind - 1, true);
            }
            else if (e.Key == NextItemKey)
            {
                e.Handled = true;
                ind = GetIndexOfItem(selected);
                MoveSelectionToIndex(ind + 1, false);
            }
            else if (e.Key == ActivateSelectedItemKey)
            {
                if (selected != null && ActivateSelectedCommand?.CanExecute(selected) == true)
                {
                    e.Handled = true;
                    ActivateSelectedCommand.Execute(selected);
                }
            }
        }

        private void MoveSelectionToIndex(int index, bool fallbackSelectLast)
        {
            if (!Items.Any())
            {
                Selected = null;
                return;
            }

            var c = Items.Count();
            if (index < 0 || index >= c)
                Selected = Items.ElementAtOrDefault(fallbackSelectLast ? c - 1 : 0);
            else
                Selected = Items.ElementAtOrDefault(index);
        }

        private int GetIndexOfItem(object itm)
        {
            var items = Items.ToList();

            if (itm == null)
                return -1;
            return items.IndexOf(itm);
        }
    }
}
