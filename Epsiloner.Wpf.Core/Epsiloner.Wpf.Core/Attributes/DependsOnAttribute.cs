using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Epsiloner.Wpf.ViewModels;

namespace Epsiloner.Wpf.Attributes
{
    /// <summary>
    /// Marks property that it changes when one of provided properties changed.
    /// Works only when other properties changes their value using <see cref="ViewModel.Set{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DependsOnAttribute : Attribute
    {
        /// <summary>
        /// List of properties that will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for marked property.
        /// </summary>
        public IReadOnlyList<string> Properties { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="properties"></param>
        public DependsOnAttribute(params string[] properties)
        {
            Properties = properties.ToList();
        }
    }
}