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

    public static SolitaireChromosome BestSoFar()
    {
        var best = new SolitaireChromosome(Random.Shared);

        // Evaluating the Position 
        best.MutableStatsByName[LegalMoveWeightName] = 1.0252;
        best.MutableStatsByName[FoundationWeightName] = 6.8471;
        best.MutableStatsByName[WasteWeightName] = -1.6529;
        best.MutableStatsByName[StockWeightName] = -0.6287;
        best.MutableStatsByName[CycleWeightName] = -0.0407;
        best.MutableStatsByName[EmptyTableauWeightName] = 0.0713;
        best.MutableStatsByName[FaceUpTableauWeightName] = 0.1376;
        best.MutableStatsByName[FaceDownTableauWeightName] = 1.9865;
        best.MutableStatsByName[ConsecutiveFaceUpTableauWeightName] = 4.7181;
        best.MutableStatsByName[FaceUpBottomCardTableauWeightName] = 1.5819;
        best.MutableStatsByName[KingIsBottomCardTableauWeightName] = 1.7532;
        best.MutableStatsByName[AceInTableauWeightName] = -1.1593;
        best.MutableStatsByName[FoundationRangeWeightName] = -0.4776;
        best.MutableStatsByName[FoundationDeviationWeightName] = -1.1048;

        // Evaluating Moves
        best.MutableStatsByName[Move_FromTableauWeightName] = 0.0;
        best.MutableStatsByName[Move_ToTableauWeightName] = 0.0;
        best.MutableStatsByName[Move_ToFoundationWeightName] = 0.0;
        best.MutableStatsByName[Move_FromFoundationWeightName] = 0.0;
        best.MutableStatsByName[Move_FromWasteWeightName] = 0.0;
        best.MutableStatsByName[Move_FromStockWeightName] = 0.0;
        best.MutableStatsByName[Move_TableaToTableauWeightName] = 0.0;

        // Skipping Games
        best.MutableStatsByName[MoveCountScalarName] = -0.4904;
        best.MutableStatsByName[Skip_FoundationCount] = -1.8681;
        best.MutableStatsByName[Skip_LegalMoveCount] = -1.7573;
        best.MutableStatsByName[Skip_ThresholdWeightName] = -1.6683;
        best.MutableStatsByName[Skip_TopWasteIsUseful] = -0.0617;
        best.MutableStatsByName[Skip_CycleWeight] = -0.3949;
        best.MutableStatsByName[Skip_StockWeight] = -1.7414;
        best.MutableStatsByName[Skip_WasteWeight] = 0.6467;
        best.MutableStatsByName[Skip_EmptyTableauCount] = -2.0881;
        best.MutableStatsByName[Skip_FaceUpTableauCount] = -0.2129;
        best.MutableStatsByName[Skip_FaceDownTableauCount] = 1.079;
        return best;
    }
}