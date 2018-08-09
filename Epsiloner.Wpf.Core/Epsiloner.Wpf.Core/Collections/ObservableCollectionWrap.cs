using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Epsiloner.Wpf.Collections
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications on specific <see cref="System.Windows.Threading.Dispatcher"/> when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class ObservableCollectionWrap<T>
            : ObservableCollection<T>
    {
        private const string IndexerName = "Item[]";
        private readonly List<ItemHandlerDelegate<T>> _handlers = new List<ItemHandlerDelegate<T>>();
        private System.Windows.Threading.Dispatcher _dispatcher;

        #region Constructors

        /// <inheritdoc />
        public ObservableCollectionWrap()
        {
        }

        /// <inheritdoc />
        public ObservableCollectionWrap(List<T> list)
            : base(list)
        {
        }

        /// <inheritdoc />
        public ObservableCollectionWrap(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <inheritdoc />
        ~ObservableCollectionWrap()
        {
            while (_handlers.Count > 0)
            {
                UnregisterHandler(_handlers.First(), true);
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Dispatcher that hosts events like <see cref="ObservableCollection{T}.CollectionChanged"/> and <see cref="ObservableCollection{T}.PropertyChanged"/>.
        /// If empty, events are raised on same thread that modified collection.
        /// </summary>
        public System.Windows.Threading.Dispatcher Dispatcher
        {
            get { return _dispatcher; }
            set
            {
                if (value == _dispatcher)
                    return;

                _dispatcher = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Registers action for each added and removed item. 
        /// </summary>
        /// <param name="handler">Action which will be invoked for each added and removed item.</param>
        /// <param name="runForExistingItems">Indicated if <paramref name="handler"/> will be invoked for existing items.</param>
        public void RegisterHandler(ItemHandlerDelegate<T> handler, bool runForExistingItems = false)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers.Add(handler);

            if (!runForExistingItems)
                return;

            var array = this.ToArray();
            for (var index = 0; index < array.Length; index++)
            {
                var item = array[index];
                RunHandlerForItem(handler, true, item, index);
            }
        }

        /// <summary>
        /// Unregisters action from each added and removed item.
        /// </summary>
        /// <param name="handler">Action which will be invoked for each added and removed item.</param>
        /// <param name="runForExistingItems">Indicated if <paramref name="handler"/> will be invoked for existing items.</param>
        public void UnregisterHandler(ItemHandlerDelegate<T> handler, bool runForExistingItems = false)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (!_handlers.Remove(handler) || !runForExistingItems)
                return;


            var array = this.ToArray();
            for (var index = 0; index < array.Length; index++)
            {
                var item = array[index];

                try
                {
                    RunHandlerForItem(handler, false, item, index);
                }
                catch {/* NEPP-1432 - Sometimes on finalizing throws exception in handler. */}
            }

        }

        /// <summary>
        /// Adds all provided objects to the end of the <see cref="ObservableCollectionWrap{T}"/>.
        /// </summary>
        /// <param name="items">The objects to be added to the end of the <see cref="ObservableCollectionWrap{T}"/>. Can contain nulls for reference types.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            AddRangeItems(items);
        }

        /// <summary>
        /// Replaces all items in collection with single item.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="ObservableCollectionWrap{T}"/>. The value can be null for reference types.</param>
        public void Replace(T item)
        {
            ReplaceRangeItems(new[] { item });
        }

        /// <summary>
        /// Replaces all items in collection with specified items.
        /// </summary>
        /// <param name="items">The objects to be added to the end of the <see cref="ObservableCollectionWrap{T}"/>. Can contain nulls for reference types.</param>
        public void ReplaceRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            ReplaceRangeItems(items);
        }

        /// <summary>
        /// Removes the first occurrences of each specified item from the <see cref="ObservableCollectionWrap{T}"/>.
        /// </summary>
        /// <param name="items">The object to remove from the System.Collections.ObjectModel.Collection`1. The value can be null for reference types.</param>
        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            RemoveRangeItems(items);
        }
        #endregion

        #region Protected Overrides
        //NOTE: MoveItem() does not causes insertion and deletion, so do not execute handlers for that method.

        /// <summary>
        /// Adds all items to the end of the <see cref="ObservableCollectionWrap{T}"/>.
        /// </summary>
        /// <param name="items">Items to insert to the end of the <see cref="ObservableCollectionWrap{T}"/>.</param>
        protected virtual void AddRangeItems(IEnumerable<T> items)
        {
            var itms = items.ToArray();


            var startIndex = Items.Count;
            for (var i = 0; i < itms.Length; i++)
            {
                Items.Insert(i + startIndex, itms[i]);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(IndexerName);

            for (var index = 0; index < itms.Length; index++)
            {
                var itm = itms[index];
                RunAllHandlersForItem(true, itm, index);
            }
        }

        /// <summary>
        /// Replaces all items in collection with specified items.
        /// </summary>
        /// <param name="items">The objects to be added to the end of the <see cref="ObservableCollectionWrap{T}"/>. Can contain nulls for reference types.</param>
        protected virtual void ReplaceRangeItems(IEnumerable<T> items)
        {
            var itms = items.ToArray();
            var old = Items.ToArray();

            Items.Clear();

            for (var i = 0; i < itms.Length; i++)
            {
                Items.Insert(i, itms[i]);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(IndexerName);

            for (var index = 0; index < old.Length; index++)
            {
                var item = old[index];
                RunAllHandlersForItem(false, item, index);
            }

            for (var index = 0; index < itms.Length; index++)
            {
                var itm = itms[index];
                RunAllHandlersForItem(true, itm, index);
            }
        }

        /// <summary>
        /// Removes the first occurrences of each specified item from the <see cref="ObservableCollectionWrap{T}"/>.
        /// </summary>
        /// <param name="items">The object to remove from the System.Collections.ObjectModel.Collection`1. The value can be null for reference types.</param>
        protected virtual void RemoveRangeItems(IEnumerable<T> items)
        {
            var itms = items.ToArray();

            var removed = itms.Where(x => Items.Remove(x)).ToList();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(IndexerName);

            for (var index = 0; index < removed.Count; index++)
            {
                var item = removed[index];
                RunAllHandlersForItem(false, item, index);
            }
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            var items = this.ToArray();

            base.ClearItems();

            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                RunAllHandlersForItem(false, item, index);
            }
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            var hasItem = index >= 0 && index < Items.Count;
            var item = Items.ElementAtOrDefault(index);

            base.RemoveItem(index);

            if (hasItem)
                RunAllHandlersForItem(false, item, index);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            RunAllHandlersForItem(true, item, index);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            var hasItem = index >= 0 && index < Items.Count;
            var old = Items.ElementAtOrDefault(index);

            base.SetItem(index, item);
            //!EqualityComparer<T>.Default.Equals(item, old)) 
            if (!ReferenceEquals(item, old))
            {
                if (hasItem)
                    RunAllHandlersForItem(false, old, index);
                RunAllHandlersForItem(true, item, index);
            }
        }

        /// <summary>
        /// Raises the <see cref="ObservableCollection{T}.PropertyChanged"/> event with the provided arguments on configured <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_dispatcher != null && !_dispatcher.HasShutdownFinished)
                _dispatcher.Invoke(() => base.OnPropertyChanged(e));
            else
                base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="ObservableCollection{T}.CollectionChanged"/> event with the provided arguments on configured <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {

            if (_dispatcher != null && !_dispatcher.HasShutdownFinished)
                _dispatcher.Invoke(() => base.OnCollectionChanged(e));
            else if (_dispatcher == null)
                base.OnCollectionChanged(e);
        }

        #endregion

        #region Private methods

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void RunAllHandlersForItem(bool inserted, T item, int index)
        {
            foreach (var handler in _handlers.ToArray())
            {
                RunHandlerForItem(handler, inserted, item, index);
            }
        }

        private void RunHandlerForItem(ItemHandlerDelegate<T> handler, bool inserted, T item, int index)
        {
            handler?.Invoke(inserted, item, index);
        }

        #endregion
    }
}
