using ScottPlot;
using SolvitaireGenetics;

namespace SolvitairePlotting;

public class AverageStatPlotStrategy : IPlottingStrategy
{
    public int Generations { get; set; } = 100;
    public double Min { get; set; } = -3;
    public double Max { get; set; } = 3;
    public void SetUpPlot(Plot plot)
    {
        plot.Clear();
        plot.XLabel("Generation");
        plot.YLabel("Stat Value");
        plot.Axes.SetLimits(0, Generations, Min, Max); 
    }

    public void UpdatePlot(Plot plot, List<GenerationLogDto> generationalLogs)
    {
        var sortedLogs = generationalLogs.OrderBy(log => log.Generation).ToList();
        var statNames = sortedLogs.First().AverageChromosome.MutableStatsByName.Keys.ToArray();

        plot.Clear();
        for (int i = 0; i < statNames.Length; i++)
        {
            var statName = statNames[i];
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