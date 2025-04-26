using System;
using System.Diagnostics;
using System.Threading;
using SolvitaireCore;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int threads = 4;
            string path = @"A:\Projects and Original Works\Solvitaire\WinnableDeals.json";
            Console.WriteLine($"Starting simulation with {threads} threads...");

            if (!File.Exists(path))
                File.WriteAllText(path, ""); // create and clear

            // Start threads
            Task[] workers = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                int threadId = i; // Capture loop variable
                workers[i] = Task.Run(() =>
                {
                    var agent = new AlphaBetaEvaluationAgent(new SecondSolitaireEvaluator());
                    var moveGenerator = new SolitaireMoveGenerator();

                    var referenceDeck = new StandardDeck(threadId*13);
                    while (true)
                    {
                        referenceDeck.Shuffle();

                        RunGameWithAgentUntilWinOrTimeout(referenceDeck, agent, moveGenerator, path, threadId);
                    }
                });
            }

            Task.WaitAll(workers); // This blocks forever unless you later add cancellation
        }

        public static void RunGameWithAgentUntilWinOrTimeout(Deck deck, SolitaireAgent agent, SolitaireMoveGenerator moveGenerator, string logFilePath, int threadId)
        {
            var gameState = new SolitaireGameState();
            gameState.DealCards(deck as StandardDeck);

            var stopwatch = Stopwatch.StartNew();
            int moveCount = 0;

            while (gameState is { IsGameWon: false, IsGameLost: false } && stopwatch.Elapsed.TotalSeconds < 1200)
            {
                var move = agent.GetNextAction(gameState);

                try
                {
                    gameState.ExecuteMove(move);
                }
                catch
                {
                    Console.WriteLine($"Illegal Move Made :( on thread: {threadId}");
                    //break; // Handle invalid moves gracefully
                }
                finally
                {
                    moveCount++;
                }
            }

            stopwatch.Stop();

            if (gameState.IsGameWon)
            {
                var json = Deck.SerializeDeck(deck);

                lock (logFilePath) // Ensure only one thread writes at a time
                {
                    File.AppendAllText(logFilePath, json + Environment.NewLine);
                }

                Console.WriteLine($"✅ Thread {threadId}: WIN after {moveCount} moves");
            }
            else
            {
                Console.WriteLine($"❌ Thread {threadId}: Loss or timeout ({moveCount} moves)");
            }
        }
    }
}