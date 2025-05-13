using CommandLine;
using CommandLine.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolvitaireIO.Database.Repositories;
using SolvitaireIO.Database;

namespace SolvitaireGenetics
{
    internal class Program
    {
        internal static string VersionNumber = "1.0.5"; // Update this as necessary
        static int Main(string[] args)
        {
            // an error code of 0 is returned if the program ran successfully.
            // otherwise, an error code of >0 is returned.
            // this makes it easier to determine via scripts when the program fails.
            int errorCode = 0;

            // Set up the command line parser
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<SolitaireGeneticAlgorithmParameters>(args);

            parserResult
                .WithParsed<SolitaireGeneticAlgorithmParameters>(options => errorCode = Run(options))
                .WithNotParsed(errs => errorCode = DisplayHelp(parserResult, errs));

            return errorCode;
        }

        private static int Run(SolitaireGeneticAlgorithmParameters options)
        {
            // Ensure the output directory exists
            if (!Directory.Exists(options.OutputDirectory))
            {
                Directory.CreateDirectory(options.OutputDirectory);
            }

            //// Build the path to the SQLite database file in the output directory
            //var databasePath = Path.Combine(options.OutputDirectory, "solvitaire.db");

            //// Set up the DI container
            //var services = new ServiceCollection();

            //// Register the DbContext with a dynamic connection string
            //services.AddDbContext<SolvitaireDbContext>(options =>
            //    options.UseSqlite($"Data Source={databasePath}"));

            //// Register the RepositoryManager
            //services.AddScoped<IRepositoryManager, RepositoryManager>();

            //// Build the service provider
            //var serviceProvider = services.BuildServiceProvider();

            //// Resolve the RepositoryManager
            //var repositoryManager = serviceProvider.GetRequiredService<IRepositoryManager>();

            // Run the genetic algorithm
            return RunAlgorithm(options, null);
        }


        private static int RunAlgorithm(SolitaireGeneticAlgorithmParameters options, IRepositoryManager repositoryManager)
        {
            // Set it all up
            GeneticSolitaireAlgorithm algorithm;
            try
            {
                // Build the Genetic Algorithm with the RepositoryManager
                algorithm = new GeneticSolitaireAlgorithm(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during setup: {ex.Message}");
                return 1; // Return a non-zero error code
            }

            // Run the genetic algorithm
            try
            {
                var configFilePath = Path.Combine(options.OutputDirectory, "RunParameters.json");
                if (File.Exists(configFilePath))
                {
                    File.Delete(configFilePath);
                }
                options.SaveToFile(configFilePath);

                algorithm.RunEvolution(options.Generations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while running the algorithm: {ex.Message}");
                return 2; // Return a non-zero error code
            }

            return 0;
        }



        private static int DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = $"Solvitaire Genetics {VersionNumber}"; // Change as necessary
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);

            Console.WriteLine(helpText);
            return 1;
        }
    }
}