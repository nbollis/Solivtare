using System.Windows.Controls;
using System.Windows.Input;

namespace SolvitaireGUI;

/// <summary>
/// Interaction logic for ConnectFourPlayingView.xaml
/// </summary>
public partial class ConnectFourPlayingView : UserControl
{
    public ConnectFourPlayingView()
    {
        InitializeComponent();
    }

    private void Cell_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border && border.Tag is int column)
            ((ConnectFourPlayingViewModel)DataContext).HoveredColumnIndex = column;
    }

    private void Cell_MouseLeave(object sender, MouseEventArgs e)
    {
        ((ConnectFourPlayingViewModel)DataContext).HoveredColumnIndex = -1;
    }

    private void Cell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is int column)
            ((ConnectFourPlayingViewModel)DataContext).MakeMoveCommand.Execute(column);
    }
}

