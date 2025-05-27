using SolvitaireCore;
using SolvitaireCore.ConnectFour;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public static class GameStateExtensions
{
    public static GameStateViewModel<TGameState, TMove> ToViewModel<TGameState, TMove>(this TGameState gameState)
        where TGameState : IGameState<TMove>
        where TMove : IMove
    {
        return gameState switch
        {
            ConnectFourGameState connectFourGameState => (GameStateViewModel<TGameState, TMove>)(object)new ConnectFourGameStateViewModel(connectFourGameState),
            TicTacToeGameState ticTacToeGameState => (GameStateViewModel<TGameState, TMove>)(object)new TicTacToeGameStateViewModel(ticTacToeGameState),
            SolitaireGameState solitaireGameState => (GameStateViewModel<TGameState, TMove>)(object)new SolitaireGameStateViewModel(solitaireGameState),
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
            case ConnectFourGameState connectFourGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(new ConnectFourHeuristicEvaluator(), 5);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomAgent<TGameState, TMove>(); 
                yield return (TAgent)(IAgent<TGameState, TMove>)new ConnectFourGeneticAgent(ConnectFourChromosome.BestSoFar(), null, 5);
                break;
            case SolitaireGameState solitaireGameState:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MaximizingAgent<SolitaireGameState, SolitaireMove>(new SecondSolitaireEvaluator(), 5);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomSolitaireAgent();
                yield return (TAgent)(IAgent<TGameState, TMove>)new SolitaireGeneticAgent(SolitaireChromosome.BestSoFar(), null, 5);
                break;
            case TicTacToeGameState ticTacToe:
                yield return (TAgent)(IAgent<TGameState, TMove>)new MinimaxAgent<TicTacToeGameState, TicTacToeMove>(new AllEqualStateEvaluator<TicTacToeGameState, TicTacToeMove>(), 5);
                yield return (TAgent)(IAgent<TGameState, TMove>)new RandomAgent<TGameState, TMove>();
                break;
        }
    }

    public static TwoPlayerGameStateViewModel<TGameState, TMove> ToTwoPlayerViewModel<TGameState, TMove>(this TGameState gameState)
        where TGameState : ITwoPlayerGameState<TMove>
        where TMove : IMove
    {
        return gameState switch
        {
            ConnectFourGameState connectFourGameState => (TwoPlayerGameStateViewModel<TGameState, TMove>)(object)new ConnectFourGameStateViewModel(connectFourGameState),
            TicTacToeGameState ticTacToeGameState => (TwoPlayerGameStateViewModel<TGameState, TMove>)(object)new TicTacToeGameStateViewModel(ticTacToeGameState),
            _ => throw new NotImplementedException($"No view model for {typeof(TGameState).Name}"),
        };
    }
}
