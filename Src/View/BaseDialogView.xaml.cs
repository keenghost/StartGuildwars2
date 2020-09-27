using StartGuildwars2.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace StartGuildwars2.View
{
    public partial class BaseDialogView : UserControl
    {
        public string DialogTitle { get; set; }
        public bool DialogShowClose { get; set; }
        public string DialogSize { get; set; }

        public static readonly DependencyProperty DialogTitleProperty = DependencyProperty.Register(
            "DialogTitle",
            typeof(string),
            typeof(BaseDialogView),
            new PropertyMetadata(OnDialogTitleChanged)
        );

        public static readonly DependencyProperty DialogShowCloseProperty = DependencyProperty.Register(
            "DialogShowClose",
            typeof(bool),
            typeof(BaseDialogView),
            new PropertyMetadata(OnDialogShowCloseChanged)
        );

        public static readonly DependencyProperty DialogSizeProperty = DependencyProperty.Register(
            "DialogSize",
            typeof(string),
            typeof(BaseDialogView),
            new PropertyMetadata(OnDialogSizeChanged)
        );

        public BaseDialogView()
        {
            InitializeComponent();
        }

        private static void OnDialogTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var v = d as BaseDialogView;
            (v.DataContext as BaseDialogViewModel).DialogTitle = (string)e.NewValue;
        }

        private static void OnDialogShowCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var v = d as BaseDialogView;
            (v.DataContext as BaseDialogViewModel).DialogShowClose = (bool)e.NewValue;
        }

        private static void OnDialogSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var v = d as BaseDialogView;
            (v.DataContext as BaseDialogViewModel).DialogSize = (string)e.NewValue;
        }
    }
}