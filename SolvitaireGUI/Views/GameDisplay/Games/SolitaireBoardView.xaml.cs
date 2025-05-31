using SolvitaireCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for SolitaireBoardView.xaml
    /// </summary>
    public partial class SolitaireBoardView : UserControl
    {
        // Track the user's selection
        private object? _selectedSource; // Can be BindableCard or BindablePile

        public SolitaireBoardView()
        {
            InitializeComponent();
        }

        // Card click handler
        private void Card_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is BindableCard card)
            {
                HandleUserClick(card);
            }
        }

        // Pile click handler (for empty piles, stock, waste, foundation)
        private void Pile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is BindablePile pile)
            {
                HandleUserClick(pile);
            }
        }

        // Main click logic
        private void HandleUserClick(object clicked)
        {
            var vm = DataContext as SolitaireGameStateViewModel;
            var controller = vm?.GameController as IGameController<SolitaireGameState, SolitaireMove>;
            if (vm == null || controller == null)
                return;

            var legalMoves = vm.BaseGameState.GetLegalMoves();

            // Special case: empty stock pile click triggers recycle if possible
            if (clicked is BindablePile pile && pile == vm.StockPile && vm.StockPile.Count == 0)
            {
                // Find a move that recycles waste to stock (waste → stock)
                var recycleMove = legalMoves.FirstOrDefault(m =>
                    m is MultiCardMove mm &&
                    mm.FromPileIndex == SolitaireGameState.WasteIndex &&
                    mm.ToPileIndex == SolitaireGameState.StockIndex);

                if (recycleMove != null)
                {
                    controller.ApplyMove(recycleMove);
                }
                return;
            }

            // Find all moves that start from the clicked card or pile
            var candidateMoves = FindMovesFromSource(vm, clicked, legalMoves).ToList();

            if (candidateMoves.Count == 1)
            {
                controller.ApplyMove(candidateMoves[0]);
            }
            else if (candidateMoves.Count > 1)
            {
                // Prefer moves to foundation, then tableau, then others
                var foundationMove = candidateMoves.FirstOrDefault(m => m.ToPileIndex >= 7 && m.ToPileIndex <= 10);
                if (foundationMove != null)
                {
                    controller.ApplyMove(foundationMove);
                    return;
                }
                var tableauMove = candidateMoves.FirstOrDefault(m => m.ToPileIndex >= 0 && m.ToPileIndex <= 6);
                if (tableauMove != null)
                {
                    controller.ApplyMove(tableauMove);
                    return;
                }
                // Otherwise, just pick the first
                controller.ApplyMove(candidateMoves[0]);
            }
            // else: no move, do nothing
        }

        // Find all legal moves that start from the clicked card or pile
        private IEnumerable<SolitaireMove> FindMovesFromSource(
            SolitaireGameStateViewModel vm,
            object source,
            IEnumerable<SolitaireMove> legalMoves)
        {
            // Helper to get pile index and card from a BindableCard or BindablePile
            static (int? pileIndex, Card? card) GetInfo(SolitaireGameStateViewModel vm, object obj)
            {
                if (obj is BindableCard card)
                {
                    // Find which pile this card is in
                    var pile = vm.TableauPiles.FirstOrDefault(p => p.Contains(card))
                        ?? vm.FoundationPiles.FirstOrDefault(p => p.Contains(card))
                        ?? (vm.WastePile.Contains(card) ? vm.WastePile : null);
                    if (pile != null)
                    {
                        int idx = GetPileIndex(vm, pile);
                        return (idx, card);
                    }
                }
                else if (obj is BindablePile pile)
                {
                    int idx = GetPileIndex(vm, pile);
                    return (idx, null);
                }
                return (null, null);
            }

            // Helper to get pile index
            static int GetPileIndex(SolitaireGameStateViewModel vm, BindablePile pile)
            {
                int idx = vm.TableauPiles.IndexOf(pile);
                if (idx >= 0) return idx; // Tableau
                idx = vm.FoundationPiles.IndexOf(pile);
                if (idx >= 0) return 7 + idx; // Foundation
                if (pile == vm.StockPile) return 11;
                if (pile == vm.WastePile) return 12;
                return -1;
            }

            var (fromIdx, fromCard) = GetInfo(vm, source);

            if (fromIdx == null)
                yield break;

            foreach (var move in legalMoves)
            {
                if (move is SingleCardMove scm)
                {
                    if (scm.FromPileIndex == fromIdx && (fromCard == null || scm.Card.Equals(fromCard)))
                        yield return move;
                }
                else if (move is MultiCardMove mcm)
                {
                    if (mcm.FromPileIndex == fromIdx)
                    {
                        // If user clicked a card, check if it's the top card of the moving set
                        if (fromCard == null || mcm.Cards.First().Equals(fromCard))
                            yield return move;
                    }
                }
            }

            // Special case: clicking stock pile to cycle
            if (source is BindablePile sp && sp == vm.StockPile)
            {
                var cycleMove = legalMoves.FirstOrDefault(m => m.FromPileIndex == 11 && m.ToPileIndex == 12);
                if (cycleMove != null)
                    yield return cycleMove;
            }
        }
    }
}
