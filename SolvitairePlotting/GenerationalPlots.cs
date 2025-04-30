using ScottPlot;
using SolvitaireGenetics;
using Color = System.Drawing.Color;

namespace SolvitairePlotting
{
    public static class PlottingConstants
    {
        public static ScottPlot.Color[] AllColors = new[]
        {
           ScottPlot.Color.FromColor(Color.DarkSeaGreen),
           ScottPlot.Color.FromColor(Color.Aqua),
           ScottPlot.Color.FromColor(Color.Coral),
           ScottPlot.Color.FromColor(Color.CornflowerBlue),
           ScottPlot.Color.FromColor(Color.Gold),
           ScottPlot.Color.FromColor(Color.LightSalmon),
           ScottPlot.Color.FromColor(Color.MediumPurple),
           ScottPlot.Color.FromColor(Color.OrangeRed),
           ScottPlot.Color.FromColor(Color.PaleVioletRed),
           ScottPlot.Color.FromColor(Color.SkyBlue),
           ScottPlot.Color.FromColor(Color.SpringGreen),
           ScottPlot.Color.FromColor(Color.Tomato),
           ScottPlot.Color.FromColor(Color.Turquoise),
           ScottPlot.Color.FromColor(Color.Violet),
           ScottPlot.Color.FromColor(Color.YellowGreen),
           ScottPlot.Color.FromColor(Color.SteelBlue),
           ScottPlot.Color.FromColor(Color.Plum),
           ScottPlot.Color.FromColor(Color.LightCoral),
           ScottPlot.Color.FromColor(Color.MediumSeaGreen),
           ScottPlot.Color.FromColor(Color.SandyBrown),
       };
    }

    public static class GenerationalPlots
    {
        public static Plot FitnessByGeneration(Plot myPlot, List<GenerationLogDto> generationalLogs, int? maxGenerations = null)
        {
            // Sort generationalLogs once to avoid repeated sorting
            var sortedLogs = generationalLogs.OrderBy(p => p.Generation).ToList();

            // Extract fitness data in a single pass
            var bestFitness = new double[sortedLogs.Count];
            var averageFitness = new double[sortedLogs.Count];
            var stdFitness = new double[sortedLogs.Count];

            for (int i = 0; i < sortedLogs.Count; i++)
            {
                bestFitness[i] = sortedLogs[i].BestFitness;
                averageFitness[i] = sortedLogs[i].AverageFitness;
                stdFitness[i] = sortedLogs[i].StdFitness;
            }

            // Add signals to the plot
            var bestSig = myPlot.Add.Signal(bestFitness);
            bestSig.LegendText = "Best Fitness";

            var avgSig = myPlot.Add.Signal(averageFitness);
            avgSig.LegendText = "Average Fitness";

            var stdSig = myPlot.Add.Signal(stdFitness);
            stdSig.LegendText = "Std Fitness";

            // Show legend
            myPlot.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);

            return myPlot;
        }

    }
}
