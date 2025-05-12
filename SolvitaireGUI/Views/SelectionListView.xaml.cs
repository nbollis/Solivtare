using System.Windows;
using System.Windows.Controls;

namespace SolvitaireGUI
{
    /// <summary>
    /// Interaction logic for SelectionListView.xaml
    /// </summary>
    public partial class SelectionListView : UserControl
    {
        public SelectionListView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty labelProperty =
            DependencyProperty.Register(
                name: "HeaderText",
                propertyType: typeof(string),
                ownerType: typeof(SelectionListView),
                typeMetadata: new FrameworkPropertyMetadata(string.Empty));

        public string HeaderText
        {
            get => (string)GetValue(labelProperty);
            set => SetValue(labelProperty, value);
        }
    }
}
