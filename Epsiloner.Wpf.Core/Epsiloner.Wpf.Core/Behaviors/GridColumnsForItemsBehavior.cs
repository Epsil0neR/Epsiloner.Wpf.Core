using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Epsiloner.Cooldowns;

namespace Epsiloner.Wpf.Behaviors
{
    /// <summary>
    /// Populates grid with specified items via <see cref="ContentControl"/> in equal columns.
    /// </summary>
    public class GridColumnsForItemsBehavior : Behavior<Grid>
    {
        /// <summary>
        /// Items for <see cref="Grid"/>.
        /// </summary>
        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(GridColumnsForItemsBehavior),
            new PropertyMetadata(null, ItemsSourceChanged));

        /// <summary>
        /// Maximum items in single row. Null -> all items in 1 row.
        /// </summary>
        public static DependencyProperty MaxInRowProperty = DependencyProperty.Register(
            nameof(MaxInRow),
            typeof(int?),
            typeof(GridColumnsForItemsBehavior),
            new PropertyMetadata(null, MaxInRowChanged));

        private readonly EventCooldown _cooldown;
        private bool _attached;

        /// <summary>
        /// (Dependency property) Items for <see cref="Grid"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// (Dependency property) Maximum items in single row. Null -> all items in 1 row.
        /// </summary>
        public int? MaxInRow
        {
            get => (int?)GetValue(MaxInRowProperty);
            set => SetValue(MaxInRowProperty, value);
        }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (GridColumnsForItemsBehavior)d;
            s?._cooldown.Now();
        }
        private static void MaxInRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (GridColumnsForItemsBehavior)d;
            s?._cooldown.Now();
        }

        public GridColumnsForItemsBehavior()
        {
            _cooldown = new EventCooldown(TimeSpan.FromMilliseconds(100), CooldownHandler);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            _attached = true;
            CleanUp();
            if (ItemsSource is INotifyCollectionChanged c)
                c.CollectionChanged += OnCollectionChanged;
            _cooldown.Now();
        }

        protected override void OnDetaching()
        {
            _attached = false;
            CleanUp();
            if (ItemsSource is INotifyCollectionChanged c)
                c.CollectionChanged -= OnCollectionChanged;
            _cooldown.Now();
            base.OnDetaching();
        }

        private void CleanUp()
        {
            AssociatedObject?.ColumnDefinitions.Clear();
            AssociatedObject?.RowDefinitions.Clear();
            AssociatedObject?.Children.Clear();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _cooldown.Accumulate();
        }

        private void CooldownHandler()
        {
            var items = ItemsSource?.Cast<object>().ToList() ?? new List<object>();
            int cols = items.Count;
            int rows = 0;

            if (MaxInRow.HasValue && MaxInRow > 0)
            {
                rows = Math.DivRem(items.Count, MaxInRow.Value, out var rem);
                if (rows > 0)
                    cols = MaxInRow.Value;
            }

            Dispatcher.Invoke(() =>
            {
                if (cols == 0 || !_attached)
                {
                    CleanUp();
                    return;
                }

                if (rows != AssociatedObject.RowDefinitions.Count)
                {
                    var rd = AssociatedObject.RowDefinitions;
                    rd.Clear();
                    for (var i = 0; i <= rows; i++)
                        rd.Add(new RowDefinition { Height = GridLength.Auto });
                }

                if (cols != AssociatedObject.ColumnDefinitions.Count)
                {
                    var cd = AssociatedObject.ColumnDefinitions;
                    cd.Clear();
                    for (int i = 0; i < cols; i++)
                        cd.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                for (var index = 0; index < items.Count; index++)
                {
                    var item = items.ElementAtOrDefault(index);
                    var cc = new ContentControl
                    {
                        Content = item
                    };

                    if (rows > 0)
                    {
                        var row = Math.DivRem(index, cols, out var col);
                        Grid.SetColumn(cc, col);
                        Grid.SetRow(cc, row);
                    }
                    else if (cols > 0)
                    {
                        Grid.SetColumn(cc, index);
                    }
                    AssociatedObject.Children.Add(cc);
                }
            });
        }
    }
}