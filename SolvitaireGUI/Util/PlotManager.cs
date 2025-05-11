using ScottPlot.WPF;
using SolvitaireGenetics;
using SolvitaireIO.Database.Models;
using SolvitairePlotting;

namespace SolvitaireGUI;

public class PlotManager
{
    public WpfPlot Plot { get; set; }
    public IPlottingStrategy PlottingStrategy { get; set; }

    public PlotManager(WpfPlot plot, IPlottingStrategy plottingStrategy)
    {
        Plot = plot;
        PlottingStrategy = plottingStrategy;
    }

    public void SetUpPlot()
    {
        lock (Plot.Plot.Sync)
        {
            Plot.Plot.Clear();
            PlottingStrategy.SetUpPlot(Plot.Plot);
            Plot.Refresh();
        }
    }

    public void UpdatePlot(List<GenerationLog> generationalLogs)
    {
        lock (Plot.Plot.Sync)
        {
            PlottingStrategy.UpdatePlot(Plot.Plot, generationalLogs);
            Plot.Refresh();
        }
    }
}