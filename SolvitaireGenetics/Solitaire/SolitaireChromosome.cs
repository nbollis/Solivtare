using SolvitaireCore;

namespace SolvitaireGenetics;

public class SolitaireChromosome : Chromosome
{
    // For position evaluation decisions
    public const string LegalMoveWeightName = "LegalMoveWeight";
    public const string FoundationWeightName = "FoundationWeight";
    public const string WasteWeightName = "WasteWeight";
    public const string StockWeightName = "StockWeight";
    public const string CycleWeightName = "CycleWeight";
    public const string EmptyTableauWeightName = "EmptyTableauWeight";
    public const string FaceUpTableauWeightName = "FaceUpTableauWeight";
    public const string FaceDownTableauWeightName = "FaceDownTableauWeight";
    public const string ConsecutiveFaceUpTableauWeightName = "ConsecutiveFaceUpTableauWeight";
    public const string FaceUpBottomCardTableauWeightName = "FaceUpBottomCardTableauWeight";
    public const string KingIsBottomCardTableauWeightName = "KingIsBottomCardTableauWeight";
    public const string AceInTableauWeightName = "AceInTableauWeight";
    public const string FoundationRangeWeightName = "FoundationRangeWeight";
    public const string FoundationDeviationWeightName = "FoundationDeviationWeight";

    // For move evaluation
    public const string Move_ToTableauWeightName = "ToTableauWeight";
    public const string Move_FromTableauWeightName = "FromTableauWeight";
    public const string Move_ToFoundationWeightName = "ToFoundationWeight";
    public const string Move_FromFoundationWeightName = "FromFoundationWeight";
    public const string Move_FromWasteWeightName = "FromWasteWeight";
    public const string Move_FromStockWeightName = "FromStockWeight";
    public const string Move_TableaToTableauWeightName = "TableaToTableauWeight"; 

    // For skip decisions. 
    public const string MoveCountScalarName = "MoveCountWeight";
    public const string Skip_ThresholdWeightName = "SkipThresholdWeight";
    public const string Skip_FoundationCount = "SkipFoundationWeight";
    public const string Skip_LegalMoveCount = "SkipLegalMoveWeight";
    public const string Skip_TopWasteIsUseful = "SkipTopWasteIsUseful";
    public const string Skip_CycleWeight = "SkipCycleWeight";
    public const string Skip_StockWeight = "SkipStockWeight";
    public const string Skip_WasteWeight = "SkipWasteWeight";
    public const string Skip_EmptyTableauCount = "SkipEmptyTableauWeight";
    public const string Skip_FaceUpTableauCount = "SkipFaceUpTableauWeight";
    public const string Skip_FaceDownTableauCount = "SkipFaceDownTableauWeight";

    public SolitaireChromosome(Random random) : base(random)
    {
        // Position evaluation weights
        MutableStatsByName[LegalMoveWeightName] = GenerateRandomWeight();
        MutableStatsByName[FoundationWeightName] = GenerateRandomWeight();
        MutableStatsByName[WasteWeightName] = GenerateRandomWeight();
        MutableStatsByName[StockWeightName] = GenerateRandomWeight();
        MutableStatsByName[CycleWeightName] = GenerateRandomWeight();
        MutableStatsByName[EmptyTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[FaceUpTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[FaceDownTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[ConsecutiveFaceUpTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[FaceUpBottomCardTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[KingIsBottomCardTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[AceInTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[FoundationRangeWeightName] = GenerateRandomWeight();
        MutableStatsByName[FoundationDeviationWeightName] = GenerateRandomWeight();

        // Move evaluation weights
        MutableStatsByName[Move_ToTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[Move_FromTableauWeightName] = GenerateRandomWeight();
        MutableStatsByName[Move_ToFoundationWeightName] = GenerateRandomWeight();
        MutableStatsByName[Move_FromFoundationWeightName] = GenerateRandomWeight();
        MutableStatsByName[Move_FromWasteWeightName] = GenerateRandomWeight();
        MutableStatsByName[Move_FromStockWeightName] = GenerateRandomWeight();
        MutableStatsByName[Move_TableaToTableauWeightName] = GenerateRandomWeight();

        // Skip evaluation weights
        MutableStatsByName[MoveCountScalarName] = GenerateRandomWeight();
        MutableStatsByName[Skip_ThresholdWeightName] = GenerateRandomWeight();
        MutableStatsByName[Skip_FoundationCount] = GenerateRandomWeight();
        MutableStatsByName[Skip_LegalMoveCount] = GenerateRandomWeight();
        MutableStatsByName[Skip_TopWasteIsUseful] = GenerateRandomWeight();
        MutableStatsByName[Skip_CycleWeight] = GenerateRandomWeight();
        MutableStatsByName[Skip_EmptyTableauCount] = GenerateRandomWeight();
        MutableStatsByName[Skip_FaceUpTableauCount] = GenerateRandomWeight();
        MutableStatsByName[Skip_FaceDownTableauCount] = GenerateRandomWeight();
        MutableStatsByName[Skip_StockWeight] = GenerateRandomWeight();
        MutableStatsByName[Skip_WasteWeight] = GenerateRandomWeight();
    }

    public SolitaireChromosome() : this(Random.Shared) { }
}

public class SolitaireGeneticAgent : MaximizingAgent<SolitaireGameState, SolitaireMove>, IGeneticAgent<SolitaireChromosome>
{
    public override string Name => "Solitaire Genetic Agent";
    public SolitaireChromosome Chromosome { get; init; }

    public SolitaireGeneticAgent(SolitaireChromosome chromosome, StateEvaluator<SolitaireGameState, SolitaireMove>? evaluator = null, int maxDepth = 3) 
        : base(evaluator ?? new GeneticSolitaireEvaluator(chromosome), maxDepth)
    {
        Chromosome = chromosome;
    }

    public IGeneticAgent<SolitaireChromosome> CrossOver(IGeneticAgent<SolitaireChromosome> other, double crossoverRate = 0.5)
        => new SolitaireGeneticAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));

    public IGeneticAgent<SolitaireChromosome> Mutate(double mutationRate)
        => new SolitaireGeneticAgent(Chromosome.Mutate<SolitaireChromosome>(mutationRate));

    public IGeneticAgent<SolitaireChromosome> Clone()
        => new SolitaireGeneticAgent(Chromosome.Clone<SolitaireChromosome>());
}