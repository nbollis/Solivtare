using System.Windows;
using System.Windows.Controls;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;
using SolvitaireCore.Gomoku;
using SolvitaireCore.TicTacToe;

namespace SolvitaireGUI;

public class GameControllerTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ConnectFourTemplate { get; set; }
    public DataTemplate? TicTacToeTemplate { get; set; }
    public DataTemplate? SolitaireTemplate { get; set; }
    public DataTemplate? GomokuTemplate { get; set; }
    // Add more as needed

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is TwoPlayerGameViewModel<ConnectFourGameState, ConnectFourMove, IAgent<ConnectFourGameState, ConnectFourMove>>)
            return ConnectFourTemplate;
        if (item is TwoPlayerGameViewModel<TicTacToeGameState, TicTacToeMove, IAgent<TicTacToeGameState, TicTacToeMove>>)
            return TicTacToeTemplate;
        if (item is OnePlayerGameViewModel<SolitaireGameState, SolitaireMove, IAgent<SolitaireGameState, SolitaireMove>>)
            return SolitaireTemplate;
        if (item is TwoPlayerGameViewModel<GomokuGameState, GomokuMove, IAgent<GomokuGameState, GomokuMove>>)
            return GomokuTemplate;

        return base.SelectTemplate(item, container);
    }
}
