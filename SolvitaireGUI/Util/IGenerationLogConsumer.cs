using SolvitaireIO.Database.Models;

namespace SolvitaireGUI;

public interface IGenerationLogConsumer
{
    void LoadGenerationLogs(IEnumerable<GenerationLog> logs);
}