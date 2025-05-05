using ScottPlot;
using SolvitaireGenetics;

namespace SolvitairePlotting;
public interface IPlottingStrategy
{
    void SetUpPlot(Plot plot);
    void UpdatePlot(Plot plot, List<GenerationLogDto> generationalLogs);
}