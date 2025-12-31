using SolvitaireCore;
using SolvitaireCore.ConnectFour;
using SolvitaireCore.Gomoku;
using SolvitaireCore.TicTacToe;
using SolvitaireCore.Wordle;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public static class GameStateExtensions
{
    public static GameStateViewModel<TGameState, TMove> ToViewModel<TGameState, TMove>(this TGameState gameState,
        IGameController<TGameState, TMove>? controller = null)
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        return gameState switch
        {
            ConnectFourGameState connectFourGameState => (GameStateViewModel<TGameState, TMove>)
                (object)new ConnectFourGameStateViewModel(connectFourGameState,
                    (IGameController<ConnectFourGameState, ConnectFourMove>)controller!),
            TicTacToeGameState ticTacToeGameState => (GameStateViewModel<TGameState, TMove>)
                (object)new TicTacToeGameStateViewModel(ticTacToeGameState,
                    (IGameController<TicTacToeGameState, TicTacToeMove>)controller!),
            SolitaireGameState solitaireGameState => (GameStateViewModel<TGameState, TMove>)
                (object)new SolitaireGameStateViewModel(solitaireGameState,
                    (IGameController<SolitaireGameState, SolitaireMove>)controller!),
            GomokuGameState gomokuGameState => (GameStateViewModel<TGameState, TMove>)
                    (object)new GomokuGameStateViewModel(gomokuGameState,
                    (IGameController<GomokuGameState, GomokuMove>)controller!),
            WordleGameState wordleGameState => (GameStateViewModel<TGameState, TMove>)
                (object)new WordleGameStateViewModel(wordleGameState,
                    (IGameController<WordleGameState, WordleMove>)controller!),
            _ => throw new NotImplementedException($"No view model for {typeof(TGameState).Name}"),
        };
    }

    public static IEnumerable<TAgent> GetPossibleAgents<TGameState, TMove, TAgent>(this TGameState gameState)
        where TGameState : IGameState<TMove>
        where TMove : IMove
        where TAgent : IAgent<TGameState, TMove>
    {
        switch (gameState)
        {
            case ConnectFourGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(new ConnectFourHeuristicEvaluator(), 5);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomAgent<TGameState, TMove>(); 
                yield return (TAgent)(IAgent<TGameState, TMove>)new ConnectFourGeneticAgent(ConnectFourChromosome.BestSoFar(), null, 5);
                break;
            case SolitaireGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MaximizingAgent<SolitaireGameState, SolitaireMove>(new SecondSolitaireEvaluator(), 5);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomSolitaireAgent();
                yield return (TAgent)(IAgent<TGameState, TMove>)new SolitaireGeneticAgent(SolitaireChromosome.BestSoFar(), null, 5);
                break;
            case TicTacToeGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MinimaxAgent<TicTacToeGameState, TicTacToeMove>(new TicTacToeHeuristicEvaluator(), 5);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomAgent<TGameState, TMove>();
                break;

            case GomokuGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MinimaxAgent<GomokuGameState, GomokuMove>(new GomokuHeuristicEvaluator(), 1);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomAgent<TGameState, TMove>();
                //yield return (TAgent)(IAgent<TGameState, TMove>)new GomokuGeneticAgent(GomokuChromosome.BestSoFar(), null, 5);
                break;
            
            case WordleGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new HeuristicWordleAgent();
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomWordleAgent();
                yield return (TAgent)(IAgent<TGameState, TMove>)new WordleGeneticAgent(WordleChromosome.BestSoFar(), null, null, 1);
                break;
        }
    }

    public static TwoPlayerGameStateViewModel<TGameState, TMove> ToTwoPlayerViewModel<TGameState, TMove>(this TGameState gameState, IGameController<TGameState, TMove>? controller = null)
        where TGameState : ITwoPlayerGameState<TMove>
        where TMove : IMove
    {
        return gameState switch
        {
            ConnectFourGameState connectFourGameState => (TwoPlayerGameStateViewModel<TGameState, TMove>)
                (object)new ConnectFourGameStateViewModel(connectFourGameState,
                    (IGameController<ConnectFourGameState, ConnectFourMove>)controller!),
            TicTacToeGameState ticTacToeGameState => (TwoPlayerGameStateViewModel<TGameState, TMove>)
                (object)new TicTacToeGameStateViewModel(ticTacToeGameState,
                    (IGameController<TicTacToeGameState, TicTacToeMove>)controller!),
            GomokuGameState gomokuGameState => (TwoPlayerGameStateViewModel<TGameState, TMove>)
                    (object)new GomokuGameStateViewModel(gomokuGameState,
                    (IGameController<GomokuGameState, GomokuMove>)controller!),
            _ => throw new NotImplementedException($"No view model for {typeof(TGameState).Name}"),
        };
    }
}
