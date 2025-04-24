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
            int threads = 6;
            string path = @"A:\Projects and Original Works\Solvitaire\WinnableDeals.json";
            Console.WriteLine($"Starting simulation with {threads} threads...");

            if (!File.Exists(path))
                File.WriteAllText(path, ""); // create and clear

            Deck[] decks = new Deck[threads];
            SolitaireAgent[] agents = new SolitaireAgent[threads];

            for (int i = 0; i < threads; i++)
            {
                decks[i] = new StandardDeck();
                agents[i] = new RandomAgent();
            }

            // Start threads
            Task[] workers = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                int threadId = i; // Capture loop variable
                workers[i] = Task.Run(() =>
                {
                    var agent = new RandomAgent();
                    var moveGenerator = new SolitaireMoveGenerator();

                    var referenceDeck = new StandardDeck(threadId);
                    while (true)
                    {
                        referenceDeck.Shuffle();
                        Deck deck = referenceDeck.Clone() as Deck;

                        RunGameWithAgentUntilWinOrTimeout(deck, agent, moveGenerator, path, threadId);
                    }
                });
            }

            Task.WaitAll(workers); // This blocks forever unless you later add cancellation
        }

        public static void RunGameWithAgentUntilWinOrTimeout(Deck deck, SolitaireAgent agent, SolitaireMoveGenerator moveGenerator, string logFilePath, int threadId)
        {
            var gameState = new SolitaireGameState();
            gameState.DealCards(deck.Clone() as StandardDeck);

            var stopwatch = Stopwatch.StartNew();
            int moveCount = 0;

            while (gameState is { IsGameWon: false, IsGameLost: false } && stopwatch.Elapsed.TotalSeconds < 30)
            {
                var moves = moveGenerator.GenerateMoves(gameState);
                var move = agent.GetNextMove(moves);

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