using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
using Epsiloner.Wpf.Extensions;

namespace Epsiloner.Wpf.Behaviors
{
    /// <summary>
    /// Applies style provided in <seealso cref="SelectedStyle"/> property to view control which represents <seealso cref="Selected"/> item.
    /// </summary>
    [ContentProperty(nameof(SelectedStyle))]
    public class ApplyStyleForSelectedBehavior : Behavior<ItemsControl>
    {
        /// <summary>
        /// Gets or sets style which will be applied for control which represents <seealso cref="SelectedProperty"/>.
        /// </summary>
        public static DependencyProperty SelectedStyleProperty = DependencyProperty.Register(nameof(SelectedStyle), typeof(Style), typeof(ApplyStyleForSelectedBehavior));

        /// <summary>
        /// Gets or sets currently selected item.
        /// </summary>
        public static DependencyProperty SelectedProperty = DependencyProperty.Register(nameof(Selected), typeof(object), typeof(ApplyStyleForSelectedBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, SeletedPropertyChangedCallback));

        /// <summary>
        /// Indicates if selected item should be bringed into view.
        /// </summary>
        public static DependencyProperty BringSelectedIntoViewProperty = DependencyProperty.Register(nameof(BringSelectedIntoView), typeof(bool), typeof(ApplyStyleForSelectedBehavior));


        private static void SeletedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ApplyStyleForSelectedBehavior)?.Preceed(e.OldValue, e.NewValue);
        }

        private readonly List<ViewState> _states = new List<ViewState>();

        /// <summary>
        /// (Dependency property) Gets or sets currently selected item.
        /// </summary>
        public object Selected
        {
            get { return GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Gets or sets style which will be applied for control which represents <seealso cref="Selected"/>.
        /// </summary>
        public Style SelectedStyle
        {
            get { return (Style)GetValue(SelectedStyleProperty); }
            set { SetValue(SelectedStyleProperty, value); }
        }

        /// <summary>
        /// (Dependency property) Indicates if selected item should be bringed into view.
        /// </summary>
        public bool BringSelectedIntoView
        {
            get { return (bool)GetValue(BringSelectedIntoViewProperty); }
            set { SetValue(BringSelectedIntoViewProperty, value); }
        }

        private void Preceed(object oldValue, object newValue)
        {
            if (oldValue != null)
            {
                var state = _states.FirstOrDefault(x => ReferenceEquals(x.Value, oldValue));
                if (state != null)
                {
                    _states.Remove(state);
                    state.Element.Style = state.OrigStyle;
                    state.Element.UpdateDefaultStyle();
                }
            }
            if (newValue != null)
            {
                var container = AssociatedObject?.ItemContainerGenerator.ContainerFromItem(newValue);
                if (container == null) return;

                if (Proceed(newValue, container as ContentPresenter)) return;
                if (Proceed(newValue, container as DataGridRow)) return;
            }
        }

        private bool Proceed(object newValue, ContentPresenter cp)
        {
            if (cp == null)
                return false;

            var element = VisualTreeHelper.GetChild(cp, 0) as FrameworkElement;
            if (element != null)
            {
                if (SelectedStyle != null)
                {
                    var styleOrig = element.Style;
                    var selectedStyle = new MultiStyleExtension(styleOrig, SelectedStyle).Merged;
                    var state = new ViewState()
                    {
                        Value = newValue,
                        Element = element,
                        OrigStyle = styleOrig,
                        SelectedStyle = selectedStyle
                    };
                    _states.Add(state);
                    element.Style = selectedStyle;
                }
                if (BringSelectedIntoView)
                    element.BringIntoView();
            }

            return true;
        }

        private bool Proceed(object newValue, DataGridRow row)
        {
            if (row == null)
                return false;

            if (SelectedStyle != null)
            {
                var styleOrig = row.Style;
                var selectedStyle = new MultiStyleExtension(styleOrig, SelectedStyle).Merged;
                var state = new ViewState()
                {
                    Value = newValue,
                    Element = row,
                    OrigStyle = styleOrig,
                    SelectedStyle = selectedStyle
                };
                _states.Add(state);
                row.Style = selectedStyle;
            }
            if (BringSelectedIntoView)
                row.BringIntoView();

            return true;
        }

        private class ViewState
        {
            public object Value { get; set; }
            public FrameworkElement Element { get; set; }
            public Style OrigStyle { get; set; }
            public Style SelectedStyle { get; set; }
        }
    }
}
