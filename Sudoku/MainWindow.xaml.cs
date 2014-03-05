using System.Windows;

namespace Sudoku
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      var viewModel = new Sudoku.ViewModels.MainViewModel();
      grid.SetupBoard(viewModel);
      DataContext = viewModel;
    }
  }
}