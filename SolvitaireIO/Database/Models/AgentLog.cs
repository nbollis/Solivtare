using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolvitaireIO.Database.Models;

[Table("agents")]
public class AgentLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("generations")]
    public int Generation { get; set; }
    public float Count { get; set; } = 1;
    public double Fitness { get; set; }
    public int GamesWon { get; set; }
    public int MovesMade { get; set; }
    public int GamesPlayed { get; set; }
    public string ChromosomeJson { get; set; } = null!;
}