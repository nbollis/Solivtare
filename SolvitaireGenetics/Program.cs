using CommandLine;
using CommandLine.Text;
using SolvitaireCore;
using SolvitaireIO;

namespace SolvitaireGenetics
{
    internal class Program
    {
        internal static string VersionNumber = "1.0.1"; // Update this as necessary
        static int Main(string[] args)
        {
            // an error code of 0 is returned if the program ran successfully.
            // otherwise, an error code of >0 is returned.
            // this makes it easier to determine via scripts when the program fails.
            int errorCode = 0;

            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<SolitaireGeneticAlgorithmParameters>(args);

            parserResult
                .WithParsed<SolitaireGeneticAlgorithmParameters>(options => errorCode = Run(options))
                .WithNotParsed(errs => errorCode = DisplayHelp(parserResult, errs));

            return errorCode;
        }

        private static int Run(SolitaireGeneticAlgorithmParameters options)
        {
            // Set it all up
            GeneticSolitaireAlgorithm algorithm;
            try
            {
                // Build the Genetic Algorithm
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
                algorithm.WriteParameters();
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