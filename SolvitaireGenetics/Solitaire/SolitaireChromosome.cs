namespace SolvitaireGenetics;

public class SolitaireChromosome : Chromosome
{
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
    public const string MoveCountScalarName = "MoveCountWeight";
    public const string SkipThresholdWeightName = "SkipThresholdWeight";
    public const string SkipFoundationWeightName = "SkipFoundationWeight";
    public const string SkipLegalMoveWeightName = "SkipLegalMoveWeight";

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
        MutableStatsByName[MoveCountScalarName] = GenerateRandomWeight();
        MutableStatsByName[SkipThresholdWeightName] = GenerateRandomWeight();
        MutableStatsByName[SkipFoundationWeightName] = GenerateRandomWeight();
        MutableStatsByName[SkipLegalMoveWeightName] = GenerateRandomWeight();
    }

    public SolitaireChromosome() : this(Random.Shared) { }
}