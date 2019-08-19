using Epsiloner.Wpf.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        #region "Static"
        private static readonly Dictionary<Type, Dictionary<string, IEnumerable<string>>> Dependencies = new Dictionary<Type, Dictionary<string, IEnumerable<string>>>();

        private static void ProcessType(Type type)
        {
            lock (Dependencies)
            {
                if (Dependencies.ContainsKey(type))
                    return;

                var t = typeof(DependsOnAttribute);
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var dict = new Dictionary<string, List<string>>();

                foreach (var prop in props)
                {
                    var attributes = prop.GetCustomAttributes(t, true);
                    foreach (var attribute in attributes)
                    {
                        var attr = (DependsOnAttribute)attribute;
                        if (attr.Properties == null)
                            continue;

                        foreach (var p in attr.Properties)
                        {
                            var list = dict.ContainsKey(p) ? dict[p] : (dict[p] = new List<string>());
                            list.Add(prop.Name);
                        }
                    }
                }

                var result = dict.ToDictionary(x => x.Key, x => x.Value.Distinct().ToList() as IEnumerable<string>);
                Dependencies[type] = result.Any() ? result : null;
            }
        }
        #endregion


        protected ViewModel()
        {
            ProcessType(GetType());
        }

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

                var t = GetType();
                Dictionary<string, IEnumerable<string>> dependencies;

                lock (Dependencies)
                    dependencies = Dependencies.ContainsKey(t) ? Dependencies[t] : null;

                if (propertyName != null && dependencies?.ContainsKey(propertyName) == true)
                    foreach (var name in dependencies[propertyName])
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
