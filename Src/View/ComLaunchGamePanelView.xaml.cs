using StartGuildwars2.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace StartGuildwars2.View
{
    public partial class ComLaunchGamePanelView : UserControl
    {
        public string GameType { get; set; }

        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
            "GameType",
            typeof(string),
            typeof(ComLaunchGamePanelView),
            new PropertyMetadata(OnGameTypeChanged)
        );

        public ComLaunchGamePanelView()
        {
            InitializeComponent();
        }

        private static void OnGameTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var v = d as ComLaunchGamePanelView;
            (v.DataContext as ComLaunchGamePanelViewModel).GameType = (string)e.NewValue;
        }
    }
}