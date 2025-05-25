using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for ConnectFourGameBoardView.xaml
    /// </summary>
    public partial class ConnectFourBoardView : UserControl
    {
        public ConnectFourBoardView()
        {
            InitializeComponent();
        }

        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is int column)
                ((ConnectFourGameStateViewModel)DataContext).HoveredColumnIndex = column;
        }

        private void Cell_MouseLeave(object sender, MouseEventArgs e)
        {
            ((ConnectFourGameStateViewModel)DataContext).HoveredColumnIndex = -1;
        }

        private void Cell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int column)
            {
                var parentVm = FindParentDataContext<TwoPlayerGameViewModel<ConnectFourGameState, ConnectFourMove, IAgent<ConnectFourGameState, ConnectFourMove>>>(this);
                if (parentVm == null)
                {
                    MessageBox.Show("Parent view model not found.");
                    return;
                }

                var move = new ConnectFourMove(column);
                parentVm.ApplyMove(move);
            }
        }

        public static T? FindParentDataContext<T>(DependencyObject child) where T : class
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if ((parent as FrameworkElement)?.DataContext is T t)
                    return t;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
