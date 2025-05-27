using System.IO;
using SolvitaireIO.Database.Models;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using SolvitaireCore.ConnectFour;
using SolvitaireGenetics;
using SolvitaireIO;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for AgentPanelView.xaml
    /// </summary>
    public partial class AgentPanelView : UserControl
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            // Add any non-generic converters here if needed
            Converters = { new ChromosomeConverter<SolitaireChromosome>(), new ChromosomeConverter<ConnectFourChromosome>(), new ChromosomeConverter<QuadraticChromosome>() },
        
        };

        public AgentPanelView()
        {
            InitializeComponent(); 
        }

        private void OnDragOverGenerationLog(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            }
        }

        private async void OnDropGenerationLog(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    var file = files[0];
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);

                        // Try to deserialize as a list of GenerationLog
                        var generations = JsonSerializer.Deserialize<List<GenerationLog>>(json, _jsonOptions);

                        if (generations != null && generations.Count > 0)
                        {
                            if (DataContext is IGenerationLogConsumer consumer)
                                consumer.LoadGenerationLogs(generations);
                        }
                        else
                        {
                            MessageBox.Show("No valid generations found in the file.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load GenerationLog: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
