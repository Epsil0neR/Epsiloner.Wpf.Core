using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sample_1.SampleApplyStyleForSelectedBehavior
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _selected;
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> Items { get; }

        public string Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public ViewModel()
        {
            Items = new List<string>()
            {
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six",
                "Seven",
                "Eight",
                "Nine",
                "Ten",
            };
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
