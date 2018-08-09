using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Epsiloner.Wpf.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged
    {
    }

    /// <summary>
    /// Base view model.
    /// </summary>
    public abstract class ViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets new value for backing field and raises <see cref="PropertyChanged"/> event for <see cref="propertyName"/> and all depending properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField">Backing field</param>
        /// <param name="newValue">new value</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="dependingPropertyNames">Depending properties</param>
        /// <returns></returns>
        protected bool Set<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = null, params string[] dependingPropertyNames)
        {
            bool valueChanged = !EqualityComparer<T>.Default.Equals(backingField, newValue);
            if (valueChanged)
            {
                backingField = newValue;
                RaisePropertyChanged(propertyName);

                if (dependingPropertyNames != null)
                    foreach (var name in dependingPropertyNames)
                        RaisePropertyChanged(name);
            }
            return valueChanged;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
