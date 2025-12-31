using SolvitaireCore;

namespace SolvitaireGenetics;

/// <summary>
/// Chromosome for Wordle genetic algorithm
/// Contains weights for different word evaluation features
/// </summary>
public class WordleChromosome : Chromosome
{
    // Core scoring weights
    public const string CorrectPositionWeightName = "CorrectPositionWeight";
    public const string PresentLetterWeightName = "PresentLetterWeight";
    public const string UnknownLetterWeightName = "UnknownLetterWeight";
    public const string AbsentLetterPenaltyName = "AbsentLetterPenalty";
    public const string WrongPositionPenaltyName = "WrongPositionPenalty";
    
    // Letter frequency and diversity weights
    public const string CommonLetterBonusName = "CommonLetterBonus";
    public const string UniqueLetterBonusName = "UniqueLetterBonus";
    public const string AllUniqueLettersBonusName = "AllUniqueLettersBonus";
    
    // Position-specific weights (for fine-tuning)
    public const string Position0WeightName = "Position0Weight";
    public const string Position1WeightName = "Position1Weight";
    public const string Position2WeightName = "Position2Weight";
    public const string Position3WeightName = "Position3Weight";
    public const string Position4WeightName = "Position4Weight";
    
    // Strategy weights
    public const string EarlyGameUnknownBonusName = "EarlyGameUnknownBonus";
    public const string MidGamePresentBonusName = "MidGamePresentBonus";
    public const string LateGameCorrectBonusName = "LateGameCorrectBonus";

    public WordleChromosome(Random random) : base(random)
    {
        // Core scoring weights
        MutableStatsByName[CorrectPositionWeightName] = GenerateRandomWeight();
        MutableStatsByName[PresentLetterWeightName] = GenerateRandomWeight();
        MutableStatsByName[UnknownLetterWeightName] = GenerateRandomWeight();
        MutableStatsByName[AbsentLetterPenaltyName] = GenerateRandomWeight();
        MutableStatsByName[WrongPositionPenaltyName] = GenerateRandomWeight();
        
        // Letter frequency and diversity
        MutableStatsByName[CommonLetterBonusName] = GenerateRandomWeight();
        MutableStatsByName[UniqueLetterBonusName] = GenerateRandomWeight();
        MutableStatsByName[AllUniqueLettersBonusName] = GenerateRandomWeight();
        
        // Position-specific weights
        MutableStatsByName[Position0WeightName] = GenerateRandomWeight();
        MutableStatsByName[Position1WeightName] = GenerateRandomWeight();
        MutableStatsByName[Position2WeightName] = GenerateRandomWeight();
        MutableStatsByName[Position3WeightName] = GenerateRandomWeight();
        MutableStatsByName[Position4WeightName] = GenerateRandomWeight();
        
        // Strategy weights
        MutableStatsByName[EarlyGameUnknownBonusName] = GenerateRandomWeight();
        MutableStatsByName[MidGamePresentBonusName] = GenerateRandomWeight();
        MutableStatsByName[LateGameCorrectBonusName] = GenerateRandomWeight();
    }

    public WordleChromosome() : this(Random.Shared) { }

    /// <summary>
    /// Creates a chromosome with heuristic-based values as starting point
    /// </summary>
    public static WordleChromosome HeuristicBased()
    {
        var chromosome = new WordleChromosome(Random.Shared);
        
        // Set to values similar to HeuristicWordleEvaluator
        chromosome.MutableStatsByName[CorrectPositionWeightName] = 100.0;
        chromosome.MutableStatsByName[PresentLetterWeightName] = 50.0;
        chromosome.MutableStatsByName[UnknownLetterWeightName] = 30.0;
        chromosome.MutableStatsByName[AbsentLetterPenaltyName] = -200.0;
        chromosome.MutableStatsByName[WrongPositionPenaltyName] = -150.0;
        
        chromosome.MutableStatsByName[CommonLetterBonusName] = 5.0;
        chromosome.MutableStatsByName[UniqueLetterBonusName] = 2.0;
        chromosome.MutableStatsByName[AllUniqueLettersBonusName] = 20.0;
        
        // Position weights (neutral initially)
        chromosome.MutableStatsByName[Position0WeightName] = 1.0;
        chromosome.MutableStatsByName[Position1WeightName] = 1.0;
        chromosome.MutableStatsByName[Position2WeightName] = 1.0;
        chromosome.MutableStatsByName[Position3WeightName] = 1.0;
        chromosome.MutableStatsByName[Position4WeightName] = 1.0;
        
        // Strategy weights
        chromosome.MutableStatsByName[EarlyGameUnknownBonusName] = 1.5;
        chromosome.MutableStatsByName[MidGamePresentBonusName] = 1.2;
        chromosome.MutableStatsByName[LateGameCorrectBonusName] = 1.3;
        
        return chromosome;
    }

    /// <summary>
    /// Best chromosome found so far (placeholder for now)
    /// </summary>
    public static WordleChromosome BestSoFar()
    {
        // Start with heuristic-based values
        // This will be updated as genetic algorithm finds better solutions
        return HeuristicBased();
    }
}
