using ScottPlot;
using SolvitaireGenetics;

namespace SolvitairePlotting;

public class GenerationStatHeatmapStrategy : IPlottingStrategy
{
    public bool PlotAverage = false;
    public int Generations { get; set; } = 100;
    public Dictionary<string, bool> StatVisibility { get; set; } = new();

    public void SetUpPlot(Plot plot)
    {
        plot.Clear();
        plot.XLabel("Generation");
        plot.YLabel("Stat Value");

        // Fix double scaling by providing proper tick positions and labels  
        var tickPositions = Enumerable.Range(0, StatVisibility.Count).Select(i => (double)i).ToArray();
        var tickLabels = StatVisibility.Keys.ToArray();
        plot.Axes.Left.SetTicks(tickPositions, tickLabels);

        plot.Axes.SetLimits(0, Generations, 0, StatVisibility.Count);
    }

    private IPlottable? previousHeatmap;
    public void UpdatePlot(Plot plot, List<GenerationLogDto> sortedLogs)
    {
        plot.Clear();

        var statNames = sortedLogs.First().AverageChromosome.MutableStatsByName.Keys.ToArray();

        // Create a 2D array to hold the stat values for the heatmap
        var statValues = new double[statNames.Length, sortedLogs.Count];
        for (int i = 0; i < statNames.Length; i++)
        {
            var statName = statNames[i];
            if (StatVisibility.TryGetValue(statName, out bool value) && !value)
                continue;
            // Fill the stat values for the heatmap
            for (int j = 0; j < sortedLogs.Count; j++)
            {
                var log = sortedLogs[j];
                var statValue = PlotAverage ? log.AverageChromosome.MutableStatsByName[statName] : log.BestChromosome.MutableStatsByName[statName];
                statValues[i, j] = statValue;
            }
        }


        var heatmap = plot.Add.Heatmap(statValues);
        heatmap.Colormap = new ScottPlot.Colormaps.Viridis();

        if (previousHeatmap is not null)
            plot.Remove(previousHeatmap);

        plot.Add.ColorBar(heatmap);
        previousHeatmap = heatmap;
    }
}

public class AverageStatLinePlotStrategy : IPlottingStrategy
{
    public int Generations { get; set; } = 100;
    public double Min { get; set; } = -3;
    public double Max { get; set; } = 3;

    public Dictionary<string, bool> StatVisibility { get; set; } = new();

    public void SetUpPlot(Plot plot)
    {
        plot.Clear();
        plot.XLabel("Generation");
        plot.YLabel("Stat Value");
        plot.Axes.SetLimits(0, Generations, Min, Max); 
    }

    public void UpdatePlot(Plot plot, List<GenerationLogDto> sortedLogs)
    {
        var statNames = sortedLogs.First().AverageChromosome.MutableStatsByName.Keys.ToArray();

        plot.Clear();
        for (int i = 0; i < statNames.Length; i++)
        {
            var statName = statNames[i];
            if (StatVisibility.TryGetValue(statName, out bool value) && !value)
                continue;

            var averageValues = sortedLogs.Select(log => log.AverageChromosome.MutableStatsByName[statName]).ToArray();
            var bestStatValues = sortedLogs.Select(log => log.BestChromosome.MutableStatsByName[statName]).ToArray();

            // Generate a consistent color for both average and best plots  
            var color = PlottingConstants.AllColors[i];

            // Add Average scatter plot for each stat  
            var scatter = plot.Add.Signal(averageValues);
            scatter.LegendText = statName;
            scatter.LinePattern = LinePattern.Solid;
            scatter.Color = color;
            scatter.LineWidth = 2;

            // Add Best scatter plot for each stat  
            var bestScatter = plot.Add.Signal(bestStatValues);
            bestScatter.LinePattern = LinePattern.DenselyDashed;
            bestScatter.Color = color;
        }

        plot.ShowLegend(Alignment.UpperLeft, Orientation.Horizontal);
    }
}