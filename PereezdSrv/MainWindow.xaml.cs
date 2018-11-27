using System.Windows;

namespace PereezdSrv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel ViewModel = new ViewModel();
        public MainWindow()
        {
            DataContext = ViewModel;
            Closed += MainWindow_Closed;
            InitializeComponent();
            ViewModel.Start(uiArgumentsGrid, uiLogger, this);
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            ViewModel.Stop();
        }

        private void PreviewGotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.UpdateCommand();
            Clipboard.SetText(ViewModel?.CurrentCommand ?? "");
        }
    }
}
