using System.Windows;
using System.Windows.Controls;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI;

public class GameControllerTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ConnectFourTemplate { get; set; }
    public DataTemplate? TicTacToeTemplate { get; set; }
    // Add more as needed

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is TwoPlayerGameViewModel<ConnectFourGameState, ConnectFourMove, IAgent<ConnectFourGameState, ConnectFourMove>>)
            return ConnectFourTemplate;
        if (item is TwoPlayerGameViewModel<TicTacToeGameState, TicTacToeMove, IAgent<TicTacToeGameState, TicTacToeMove>>)
            return TicTacToeTemplate;
        // Add more as needed
        return base.SelectTemplate(item, container);
    }
}
