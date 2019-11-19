using System.Windows;

namespace Sample_1
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApplyStyleForSelectedBehavior_OnClick(object sender, RoutedEventArgs e)
        {
            new SampleApplyStyleForSelectedBehavior.SampleApplyStyleForSelectedBehavior().Show();
        }

        private void KeyboardNavigationBehavior_OnClick(object sender, RoutedEventArgs e)
        {
            new SampleKeyboardNavigationBehavior.SampleKeyboardNavigationBehavior().Show();
        }

        private void MultiStyleExtension_OnClick(object sender, RoutedEventArgs e)
        {
            new SampleMultiStyleExtension.SampleMultiStyleExtension().Show();
        }

        private void GridColumnsForItemsBehavior_OnClick(object sender, RoutedEventArgs e)
        {
            new SampleSmartGridBehavior.SampleSmartGridBehavior().Show();
        }
    }
}
