using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Sample_1.SampleKeyboardNavigationBehavior
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _selected;
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> Items { get; }
        public ICommand ActivateCommand { get; }

        public string Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged();
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

            ActivateCommand = new RelayCommand(Activate);
        }

        private void Activate(object obj)
        {
            MessageBox.Show("Activating: " + obj);
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
