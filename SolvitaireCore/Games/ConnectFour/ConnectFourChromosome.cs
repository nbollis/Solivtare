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
    public const string PlayerTouchingWeight = "PlayerTouching";
    public const string OpponentTouchingWeight = "OpponentTouching";

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
        MutableStatsByName[PlayerTouchingWeight] = GenerateRandomWeight();
        MutableStatsByName[OpponentTouchingWeight] = GenerateRandomWeight();

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

    public static ConnectFourChromosome BestSoFar()
    {
        var chromosome = new ConnectFourChromosome();
        // Set weights to known good values  
        chromosome.MutableStatsByName[SurroundedPieceWeight] = 0.08;
        chromosome.MutableStatsByName[IsolatedPieceWeight] = -0.78;
        chromosome.MutableStatsByName[TwoInARowWeight] = -2.80;
        chromosome.MutableStatsByName[TwoWithOneGapWeight] = 2.67;
        chromosome.MutableStatsByName[ThreeInARowWeight] = -0.12;
        chromosome.MutableStatsByName[ThreeWithOneGapWeight] = 2.24;
        chromosome.MutableStatsByName[PlayerTouchingWeight] = 0.0;
        chromosome.MutableStatsByName[OpponentTouchingWeight] = 0.0;


        chromosome.MutableStatsByName[ColumnOneWeight] = 2.11;
        chromosome.MutableStatsByName[ColumnTwoWeight] = -1.24;
        chromosome.MutableStatsByName[ColumnThreeWeight] = -0.09;
        chromosome.MutableStatsByName[ColumnFourWeight] = 1.29;
        chromosome.MutableStatsByName[ColumnFiveWeight] = 1.78;
        chromosome.MutableStatsByName[ColumnSixWeight] = 1.91;
        chromosome.MutableStatsByName[ColumnSevenWeight] = 1.43;
        chromosome.MutableStatsByName[RowOneWeight] = 1.54;
        chromosome.MutableStatsByName[RowTwoWeight] = 0.79;
        chromosome.MutableStatsByName[RowThreeWeight] = -1.18;
        chromosome.MutableStatsByName[RowFourWeight] = 3.22;
        chromosome.MutableStatsByName[RowFiveWeight] = -0.48;
        chromosome.MutableStatsByName[RowSixWeight] = -0.46;

        return chromosome;
    }
}
