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
            if (args.Any())
            {
                if (args.Length == 1)
                {
                    int seconds = int.Parse(args[0]);
                    var gameState = new SolitaireGameState();
                    var agent = new MaxiMaxAgent(new SecondSolitaireEvaluator());
                    var deck = new StandardDeck();
                    deck.Shuffle();

                    var simulation = new AgentSimulation(agent, deck);
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;

                    // Start simulation in a separate task  
                    var simulationTask = Task.Run(() => simulation.RunAgentSimulation(gameState, cancellationToken), cancellationToken);

                    // Cancel after the specified number of seconds  
                    Task.Delay(TimeSpan.FromSeconds(seconds)).ContinueWith(_ => cancellationTokenSource.Cancel());

                    try
                    {
                        simulationTask.Wait(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        var result = simulationTask.Result;
                        Console.WriteLine($"Simulation completed: {result.GamesPlayed} games played, {result.GamesWon} wins, {result.MovesPlayed} moves.");
                        Console.WriteLine("Simulation canceled after timeout.");
                    }

                    //Console.ReadLine();
                }
            }
            else
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
                        var agent = new MaxiMaxAgent(new SecondSolitaireEvaluator());
                        var moveGenerator = new SolitaireMoveGenerator();

                        var referenceDeck = new StandardDeck(threadId * 11);
                        while (true)
                        {
                            referenceDeck.Shuffle();

                            RunGameWithAgentUntilWinOrTimeout(referenceDeck, agent, moveGenerator, path, threadId);
                        }
                    });
                }

                Task.WaitAll(workers); // This blocks forever unless you later add cancellation  
            }

            
        }

        public static void RunGameWithAgentUntilWinOrTimeout(Deck deck, SolitaireAgent agent,
            SolitaireMoveGenerator moveGenerator, string logFilePath, int threadId, int timeout = 500)
        {
            var gameState = new SolitaireGameState();
            gameState.DealCards(deck as StandardDeck);

            var stopwatch = Stopwatch.StartNew();
            int moveCount = 0;

            while (gameState is { IsGameWon: false} && stopwatch.Elapsed.TotalSeconds < timeout)
            {
                var decision = agent.GetNextAction(gameState);

                try
                {
                    if (decision.ShouldSkipGame)
                    {
                        Console.WriteLine($"Thread {threadId} skipped the game.");
                        break;
                    }

                    if (decision.Move == null)
                    {
                        Console.WriteLine($"Thread {threadId} made no move.");
                        break;
                    }

                    gameState.ExecuteMove(decision.Move);
                }
                catch
                {
                    Console.WriteLine($"Illegal Move Made :( on thread: {threadId}");
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