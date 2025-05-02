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
           ScottPlot.Color.FromColor(Color.DarkOrchid),
           ScottPlot.Color.FromColor(Color.SpringGreen),
           ScottPlot.Color.FromColor(Color.Tomato),
           ScottPlot.Color.FromColor(Color.Peru),
           ScottPlot.Color.FromColor(Color.Violet),
           ScottPlot.Color.FromColor(Color.YellowGreen),
           ScottPlot.Color.FromColor(Color.SteelBlue),
           ScottPlot.Color.FromColor(Color.Plum),
           ScottPlot.Color.FromColor(Color.LightCoral),
           ScottPlot.Color.FromColor(Color.MediumSeaGreen),
           ScottPlot.Color.FromColor(Color.SandyBrown),
       };
    }



    public abstract class GenerationalScottPlot
    {
        public Plot GetPlot(List<GenerationLogDto> generationalLogs)
        {
            var plot = new Plot();
            SetUp(plot);
            Plot(plot, generationalLogs);
            return plot;
        }

        public abstract void SetUp(Plot myPlot);
        public abstract void Plot(Plot myPlot, List<GenerationLogDto> generationalLogs);
    }

    public class FitnessByGenerationPlot
    {
        public virtual void SetUp(Plot myPlot)
        {
            myPlot.XLabel("Generation");
            myPlot.YLabel("Fitness");
        }

        public virtual void Plot(Plot myPlot, List<GenerationLogDto> generationalLogs)
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


            myPlot.Clear();
            myPlot.Axes.SetLimits(0, sortedLogs.Last().Generation + 1, 0, 1);

            // Add signals to the plot
            var bestSig = myPlot.Add.Signal(bestFitness);
            bestSig.LegendText = "Best Fitness";

            var avgSig = myPlot.Add.Signal(averageFitness);
            avgSig.LegendText = "Average Fitness";

            var stdSig = myPlot.Add.Signal(stdFitness);
            stdSig.LegendText = "Std Fitness";

            // Show legend
            myPlot.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
        }
    }
}
