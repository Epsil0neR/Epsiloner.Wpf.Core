using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Epsiloner.Wpf.Collections
{
    /// <summary>
    /// Represents a read-only collection with custom dispatcher host for events.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class ReadOnlyObservableCollectionWrap<T>
        : ReadOnlyObservableCollection<T>
    {
        private readonly ObservableCollectionWrap<T> _observableCollectionWrap;
        private System.Windows.Threading.Dispatcher _dispatcher;

        #region Properties
        /// <summary>
        /// Dispatcher that hosts events like <see cref="ReadOnlyObservableCollection{T}.CollectionChanged"/> and <see cref="ReadOnlyObservableCollection{T}.PropertyChanged"/>.
        /// Can be different from <see cref="ObservableCollectionWrap{T}.Dispatcher"/>.
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

        /// <inheritdoc />
        public ReadOnlyObservableCollectionWrap(ObservableCollectionWrap<T> list) 
            : base(list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            _observableCollectionWrap = list;
        }

        /// <summary>
        /// Occurs when an item is added or removed.
        /// </summary>
        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { base.CollectionChanged += value; }
            remove { base.CollectionChanged -= value; }
        }

        #region Public methods

        /// <summary>
        /// Registers action for each added and removed item. 
        /// </summary>
        /// <param name="handler">Action which will be invoked for each added and removed item.</param>
        /// <param name="runForExistingItems">Indicated if <paramref name="handler"/> will be invoked for existing items.</param>
        public void RegisterHandler(ItemHandlerDelegate<T> handler, bool runForExistingItems = false)
        {
            _observableCollectionWrap.RegisterHandler(handler, runForExistingItems);
        }

        /// <summary>
        /// Unregisters action from each added and removed item.
        /// </summary>
        /// <param name="handler">Action which will be invoked for each added and removed item.</param>
        /// <param name="runForExistingItems">Indicated if <paramref name="handler"/> will be invoked for existing items.</param>
        public void UnregisterHandler(ItemHandlerDelegate<T> handler, bool runForExistingItems = false)
        {
            _observableCollectionWrap.UnregisterHandler(handler, runForExistingItems);
        }
        #endregion

        /// <summary>
        /// Raises the <see cref="ReadOnlyObservableCollection{T}.PropertyChanged"/> event with the provided arguments on configured <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_dispatcher != null && !_dispatcher.HasShutdownFinished)
            {
                if (_dispatcher.CheckAccess())
                    base.OnPropertyChanged(e);
                else
                    _dispatcher.InvokeAsync(() => base.OnPropertyChanged(e));
            }
            else
            {
                base.OnPropertyChanged(e);
            }
        }


        /// <summary>
        /// Raises the <see cref="ReadOnlyObservableCollection{T}.CollectionChanged"/> event with the provided arguments on configured <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_dispatcher != null && !_dispatcher.HasShutdownFinished)
            {
                if (_dispatcher.CheckAccess())
                    base.OnCollectionChanged(e);
                else
                    _dispatcher.InvokeAsync(() => base.OnCollectionChanged(e));
            }
            else if (_dispatcher == null)
            {
                base.OnCollectionChanged(e);
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}