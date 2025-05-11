using ScottPlot;
using SolvitaireGenetics;
using SolvitaireIO.Database.Models;

namespace SolvitairePlotting;
public interface IPlottingStrategy
{
    void SetUpPlot(Plot plot);
    void UpdatePlot(Plot plot, List<GenerationLog> generationalLogs);
}