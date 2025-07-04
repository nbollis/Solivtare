using ScottPlot;
using SolvitaireGenetics;
using SolvitaireIO.Database.Models;

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

    public void UpdatePlot(Plot plot, List<GenerationLog> generationalLogs)
    {
        var sortedLogs = generationalLogs.OrderBy(log => log.Generation).ToList();

        var bestFitness = sortedLogs.Select(log => log.BestFitness).ToArray();
        var averageFitness = sortedLogs.Select(log => log.AverageFitness).ToArray();
        var stdFitness = sortedLogs.Select(log => log.StdFitness).ToArray();
        var speciesCount = sortedLogs.Select(log => log.SpeciesCount).ToArray();
        var averagePairwiseDiversity = sortedLogs.Select(log => log.AveragePairwiseDiversity).ToArray();

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



        //var diversitySig = plot.Add.Signal(averagePairwiseDiversity);
        //diversitySig.LegendText = "Genetic Diversity";
        //diversitySig.LineWidth = 1;

        plot.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
    }
}