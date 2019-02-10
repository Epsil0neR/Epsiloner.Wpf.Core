using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

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
                _dispatcher.Invoke(() => base.OnCollectionChanged(e));
            else if (_dispatcher == null)
                base.OnCollectionChanged(e);


        }
        #endregion
    }
}
