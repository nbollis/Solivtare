namespace SolvitaireCore.ConnectFour;

public class ConnectFourChromosome : Chromosome
{
    // State Evaluations  
    public const string SurroundedPieceWeight = "SurroundedPiece";
    public const string IsolatedPieceWeight = "IsolatedPiece";
    public const string TwoInARowWeight = "TwoInARow";
    public const string TwoWithOneGapWeight = "TwoWithOneGap";
    public const string ThreeInARowWeight = "ThreeInARow";
    public const string ThreeWithOneGapWeight = "ThreeWithOneGap";

    // Move Evaluations    
    public const string ColumnOneWeight = "Move_ColumnOne";
    public const string ColumnTwoWeight = "Move_ColumnTwo";
    public const string ColumnThreeWeight = "Move_ColumnThree";
    public const string ColumnFourWeight = "Move_ColumnFour";
    public const string ColumnFiveWeight = "Move_ColumnFive";
    public const string ColumnSixWeight = "Move_ColumnSix";
    public const string ColumnSevenWeight = "Move_ColumnSeven";

    public const string RowOneWeight = "Move_RowOne";
    public const string RowTwoWeight = "Move_RowTwo";
    public const string RowThreeWeight = "Move_RowThree";
    public const string RowFourWeight = "Move_RowFour";
    public const string RowFiveWeight = "Move_RowFive";
    public const string RowSixWeight = "Move_RowSix";

    public ConnectFourChromosome(Random random) : base(random)
    {
        // State evaluation weights  
        MutableStatsByName[SurroundedPieceWeight] = GenerateRandomWeight();
        MutableStatsByName[IsolatedPieceWeight] = GenerateRandomWeight();
        MutableStatsByName[TwoInARowWeight] = GenerateRandomWeight();
        MutableStatsByName[TwoWithOneGapWeight] = GenerateRandomWeight();
        MutableStatsByName[ThreeInARowWeight] = GenerateRandomWeight();
        MutableStatsByName[ThreeWithOneGapWeight] = GenerateRandomWeight();

        // Move evaluation weights  
        MutableStatsByName[ColumnOneWeight] = GenerateRandomWeight();
        MutableStatsByName[ColumnTwoWeight] = GenerateRandomWeight();
        MutableStatsByName[ColumnThreeWeight] = GenerateRandomWeight();
        MutableStatsByName[ColumnFourWeight] = GenerateRandomWeight();
        MutableStatsByName[ColumnFiveWeight] = GenerateRandomWeight();
        MutableStatsByName[ColumnSixWeight] = GenerateRandomWeight();
        MutableStatsByName[ColumnSevenWeight] = GenerateRandomWeight();

        MutableStatsByName[RowOneWeight] = GenerateRandomWeight();
        MutableStatsByName[RowTwoWeight] = GenerateRandomWeight();
        MutableStatsByName[RowThreeWeight] = GenerateRandomWeight();
        MutableStatsByName[RowFourWeight] = GenerateRandomWeight();
        MutableStatsByName[RowFiveWeight] = GenerateRandomWeight();
        MutableStatsByName[RowSixWeight] = GenerateRandomWeight();
    }

    public ConnectFourChromosome() : this(Random.Shared) { }
}
