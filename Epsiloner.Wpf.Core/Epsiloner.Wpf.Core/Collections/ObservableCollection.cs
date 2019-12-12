using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace Epsiloner.Wpf.Collections
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications on specific <see cref="System.Windows.Threading.Dispatcher"/> when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class ObservableCollection<T>
            : Epsiloner.Collections.ObservableCollection<T>
    {
        private System.Windows.Threading.Dispatcher _dispatcher;

        #region Constructors
        /// <inheritdoc />
        public ObservableCollection()
        {
        }

        /// <inheritdoc />
        public ObservableCollection(List<T> list)
            : base(list)
        {
        }

        /// <inheritdoc />
        public ObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// Dispatcher that hosts events like <see cref="Epsiloner.Collections.ObservableCollection{T}.CollectionChanged"/> and <see cref="Epsiloner.Collections.ObservableCollection{T}.PropertyChanged"/>.
        /// If empty, events are raised on same thread that modified collection.
        /// </summary>
        public System.Windows.Threading.Dispatcher Dispatcher
        {
            get => _dispatcher;
            set
            {
                if (value == _dispatcher)
                    return;

                _dispatcher = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Protected Overrides
        /// <summary>
        /// Raises the <see cref="Epsiloner.Collections.ObservableCollection{T}.PropertyChanged"/> event with the provided arguments on configured <see cref="Dispatcher"/>.
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
        /// Raises the <see cref="Epsiloner.Collections.ObservableCollection{T}.CollectionChanged"/> event with the provided arguments on configured <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {

            if (_dispatcher != null && !_dispatcher.HasShutdownFinished)
                _dispatcher.Invoke(() => OnCollectionChangeAction(e));
            else if (_dispatcher == null)
                OnCollectionChangeAction(e);
        }


        /// <summary>Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.</summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Wrapper for extra logic to raise correctly <see cref="OnCollectionChanged"/>.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Improved taken from:
        /// https://stackoverflow.com/questions/3300845/observablecollection-calling-oncollectionchanged-with-multiple-new-items
        /// </remarks>
        protected virtual void OnCollectionChangeAction(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged == null)
                return;

            using (BlockReentrancy())
            {
                if (e.NewItems == null || e.NewItems.Count <= 1)
                {
                    CollectionChanged?.Invoke(this, e);
                }
                else
                {
                    var handlers = CollectionChanged;
                    if (handlers == null) 
                        return;
                    foreach (var @delegate in handlers.GetInvocationList())
                    {
                        var handler = @delegate as NotifyCollectionChangedEventHandler;
                        if (handler == null)
                            continue;

                        if (handler.Target is CollectionView cv)
                            cv.Refresh();
                        else
                            handler(this, e);
                    }
                }
            }
        }
        #endregion
    }
}
