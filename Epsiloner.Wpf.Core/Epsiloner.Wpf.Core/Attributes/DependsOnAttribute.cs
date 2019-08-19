using System;
using System.Collections.Generic;
using System.Linq;

namespace Epsiloner.Wpf.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependsOnAttribute : Attribute
    {
        public IReadOnlyList<string> Properties { get; set; }

        public DependsOnAttribute(params string[] properties)
        {
            Properties = properties.ToList();
        }
    }
}