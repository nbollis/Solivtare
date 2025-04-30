using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for GeneticAlgorithmTabView.xaml
    /// </summary>
    public partial class GeneticAlgorithmTabView : UserControl
    {
        public GeneticAlgorithmTabView()
        {
            InitializeComponent();
        }

        private void ParametersExpander_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Any(file => Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void ParametersExpander_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                (DataContext as GeneticAlgorithmViewModel)?.HandleFileDrop(e.Data.GetData(DataFormats.FileDrop) as string[]);

            }
        }
    }
}
