namespace SolvitaireCore.Wordle;

/// <summary>
/// Represents the feedback for a letter in a Wordle guess
/// </summary>
public enum LetterFeedback
{
    /// <summary>
    /// Letter is not in the target word
    /// </summary>
    Absent = 0,
    
    /// <summary>
    /// Letter is in the target word but in wrong position
    /// </summary>
    Present = 1,
    
    /// <summary>
    /// Letter is in the target word and in correct position
    /// </summary>
    Correct = 2
}
