using SolvitaireCore;

namespace SolvitaireGUI;

    public class MoveViewModel : BaseViewModel
    {
        public SolitaireMove Move { get; }
        public string MoveString { get; set; }
        public double Evaluation { get; set; }


        public MoveViewModel(SolitaireMove move, double eval)
        {
            Move = move;
            MoveString = move.ToString();
            Evaluation = eval;
        }
    }

