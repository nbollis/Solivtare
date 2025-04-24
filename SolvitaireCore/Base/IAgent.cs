namespace SolvitaireCore;


public interface IAgent
{
    string Name { get; }
}

public interface IAgent<TMove> : IAgent where TMove : IMove
{
    public TMove GetNextMove(IEnumerable<TMove> legalMoves);
}