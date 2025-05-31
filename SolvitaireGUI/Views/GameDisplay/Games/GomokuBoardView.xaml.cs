using SolvitaireCore.Gomoku;
using SolvitaireCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for GomokuBoardView.xaml
    /// </summary>
    public partial class GomokuBoardView : UserControl
    {
        public GomokuBoardView()
        {
            InitializeComponent();
        }

        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is int flatIndex)
            {
                var vm = (GomokuGameStateViewModel)DataContext;
                int n = vm.BoardSize;
                vm.HoveredRowIndex = flatIndex / n;
                vm.HoveredColumnIndex = flatIndex % n;
            }
        }

        private void Cell_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (GomokuGameStateViewModel)DataContext;
            vm.HoveredRowIndex = -1;
            vm.HoveredColumnIndex = -1;
        }

        private void Cell_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int flatIndex)
            {
                var parentVm = FindParentDataContext<TwoPlayerGameViewModel<GomokuGameState, GomokuMove, IAgent<GomokuGameState, GomokuMove>>>(this);
                if (parentVm == null)
                {
                    MessageBox.Show("Parent view model not found.");
                    return;
                }

                var vm = (GomokuGameStateViewModel)DataContext;
                int n = vm.BoardSize;
                int row = flatIndex / n;
                int col = flatIndex % n;

                if (parentVm.GameStateViewModel.GameState.Board[row, col] != 0)
                    return;

                var move = new GomokuMove(row, col);
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
