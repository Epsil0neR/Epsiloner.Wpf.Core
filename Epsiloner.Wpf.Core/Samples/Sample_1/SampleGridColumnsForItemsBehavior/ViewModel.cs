using System.Windows.Controls;
using Epsiloner.Collections;

namespace Sample_1.SampleGridColumnsForItemsBehavior
{
    public class ViewModel : Epsiloner.Wpf.ViewModels.ViewModel
    {
        public ObservableCollection<TextBlock> TextBlocks { get; }
        public ReadOnlyObservableCollection<TextBlock> Items { get; }

        public ViewModel()
        {
            TextBlocks = new ObservableCollection<TextBlock>();

            for (var i = 1; i <= 100; i++)
                TextBlocks.Add(new TextBlock { Text = i.ToString() });

            Items = new ReadOnlyObservableCollection<TextBlock>(TextBlocks);
        }
    }
}
