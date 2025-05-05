using ScottPlot;
using SolvitaireGenetics;

namespace SolvitairePlotting;

public class FitnessPlotStrategy : IPlottingStrategy
{
    public int Generations { get; set; } = 100;
    public double Min { get; set; } = 0;
    public double Max { get; set; } = 1;

    public void SetUpPlot(Plot plot)
    {
        plot.Clear();
        plot.XLabel("Generation");
        plot.YLabel("Fitness");
        plot.Axes.SetLimits(0, Generations, Min, Max); // Example limits
    }

    public void UpdatePlot(Plot plot, List<GenerationLogDto> generationalLogs)
    {
        var sortedLogs = generationalLogs.OrderBy(log => log.Generation).ToList();

        var bestFitness = sortedLogs.Select(log => log.BestFitness).ToArray();
        var averageFitness = sortedLogs.Select(log => log.AverageFitness).ToArray();
        var stdFitness = sortedLogs.Select(log => log.StdFitness).ToArray();

        plot.Clear();

        var bestSig = plot.Add.Signal(bestFitness);
        bestSig.LegendText = "Best Fitness";
        bestSig.LineWidth = 2;

        var avgSig = plot.Add.Signal(averageFitness);
        avgSig.LegendText = "Average Fitness";
        avgSig.LineWidth = 2;

        var stdSig = plot.Add.Signal(stdFitness);
        stdSig.LegendText = "Std Fitness";
        stdSig.LineWidth = 2;

        plot.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
    }
}