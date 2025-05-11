namespace SolvitaireGenetics;

public class SolitaireChromosome : Chromosome
{
    // For move decisions
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