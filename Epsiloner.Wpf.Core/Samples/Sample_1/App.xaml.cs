using System;
using System.Windows;

namespace Sample_1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <inheritdoc />
        public App()
        {
            Test(typeof(Class1));
            Test(typeof(Class1));
            var t = Epsiloner.Wpf.Controls.ViewDataTemplateSelector.Instance;
        }

        void Test(Type type)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
}
