namespace SolvitaireCore;

public interface IAgent<in TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    string Name { get; }
    public TMove GetNextAction(TGameState gameState); 
    public double EvaluateMoveWithAgent(TGameState gameState, TMove move, int? perspectivePlayer = null);
    public void ResetState();
}

public interface ISearchAgent<TGameState, TMove> : IAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public int MaxDepth { get; set; }
    public StateEvaluator<TGameState, TMove> Evaluator { get;  }
}