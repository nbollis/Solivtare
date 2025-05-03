namespace SolvitaireGenetics;

public class QuadraticChromosome : Chromosome
{
    public const string A = "a";
    public const string B = "b";
    public const string C = "c";
    public const string YIntercept = "yIntercept";
    public const string EvalFunction = "evalFunction";
    public QuadraticChromosome(Random random) : base(random)
    {
        MutableStatsByName[A] = GenerateRandomWeight();
        MutableStatsByName[B] = GenerateRandomWeight();
        MutableStatsByName[C] = GenerateRandomWeight();
        MutableStatsByName[YIntercept] = GenerateRandomWeight();
        MutableStatsByName[EvalFunction] = GenerateRandomWeight();
        CanFullRandomMutate = false;
        WeightMinStartValue = -4;
        WeightMaxStartValue = 4;
    }
    public QuadraticChromosome() : this(Random.Shared) { }
    public double Get(string name) => MutableStatsByName.TryGetValue(name, out var v) ? v : 0;
    public void Set(string name, double value) => MutableStatsByName[name] = value;
}