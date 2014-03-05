using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Sudoku.Models;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Sudoku.ViewModels.Base;

namespace Sudoku.Controls
{
  public class SudokuGrid : Grid
  {
    private SolidColorBrush disbleBrush = new SolidColorBrush(Colors.Navy);
    public void SetupBoard(Sudoku.ViewModels.MainViewModel viewModel)
    {
      if (viewModel == null)
        throw new ArgumentNullException("viewModel");
      var board = viewModel.Board;
      if (board == null)
        throw new ArgumentNullException("board");

      SetupGrid(board);
      SetupTextboxes(viewModel);
    }

    private void SetupGrid(Board board)
    {
      this.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 2, MaxWidth = 2 });
      this.RowDefinitions.Add(new RowDefinition { MinHeight = 2, MaxHeight = 2 });
      for (int counter = 0; counter < board.Size; counter++)
      {
        this.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 40, MaxWidth = 40 });
        this.RowDefinitions.Add(new RowDefinition { MinHeight = 40, MaxHeight = 40 });
        if (counter % board.SquareSize == board.SquareSize - 1)
        {
          this.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 2, MaxWidth = 2 });
          this.RowDefinitions.Add(new RowDefinition { MinHeight = 2, MaxHeight = 2 });
        }
      }
    }

    private void SetupTextboxes(Sudoku.ViewModels.MainViewModel viewModel)
    {
      Board board = viewModel.Board;
      var rangeRule = new ValidationRules.BoardRange(board);
      for (int row = 1; row <= board.Size; row++)
      {
        var gridRow = row + (row / board.SquareSize) - (row % board.SquareSize == 0 ? 1 : 0);
        for (int column = 1; column <= board.Size; column++)
        {
          var gridColumn = column + (column / board.SquareSize) - (column % board.SquareSize == 0 ? 1 : 0);

          var textbox = new TextBox
          {
            Height = 40,
            Width = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 20
          };
          Grid.SetColumn(textbox, gridColumn);
          Grid.SetRow(textbox, gridRow);

          AssignShortcut(textbox, row, column);

          var cell = board[row, column];
          textbox.DataContext = cell;

          textbox.PreviewLostKeyboardFocus += (sender, e) =>
          {
            var tb = sender as TextBox;
            int value;
            if (!string.IsNullOrEmpty(tb.Text)
              && !(int.TryParse(tb.Text, out value) && cell.IsValid(value)))
              e.Handled = true;
            else
              tb.ToolTip = null;
          };

          textbox.IsEnabled = cell.Enabled;
          if (!cell.Enabled)
            textbox.Foreground = disbleBrush;

          Children.Add(textbox);
          var textbinding = new Binding
          {
            Mode = BindingMode.TwoWay,
            Path = new PropertyPath("Value"),
            TargetNullValue = string.Empty,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            //ValidatesOnDataErrors = true,
            //NotifyOnValidationError = true,
            NotifyOnTargetUpdated = true,
            UpdateSourceExceptionFilter = (expression, exception) =>
              {
                viewModel.Notify(exception.Message);
                textbox.ToolTip = exception.Message;
                return expression;
              }
          };
          textbinding.ValidationRules.Add(rangeRule);
          textbox.SetBinding(TextBox.TextProperty, textbinding);
        }
      }

      AssignShortcutsToWindow();
    }

    private void AssignShortcutsToWindow()
    {
      FrameworkElement root = this.Parent as FrameworkElement;
      while (root.Parent != null)
      {
        root = root.Parent as FrameworkElement;
      }
      root.InputBindings.AddRange(InputBindings);
    }

    private void AssignShortcut(TextBox textbox, int row, int column)
    {
      var hotKey = new KeyBinding(new RelayCommand(() => textbox.Focus()), new MultiKeyGesture(new List<Key> { (Key)(row + 34), (Key)(column + 34) }, ModifierKeys.Control | ModifierKeys.Shift));
      this.InputBindings.Add(hotKey);
      if (row < 10 && column < 10)
      {
        hotKey = new KeyBinding(new RelayCommand(() => textbox.Focus()), new MultiKeyGesture(new List<Key> { (Key)(row + 34), (Key)(column + 34) }, ModifierKeys.Control));
        this.InputBindings.Add(hotKey);
        // following is not working
        //hotKey = new KeyBinding(new RelayCommand(() => textbox.Focus()), new MultiKeyGesture(new List<Key> { (Key)(row + 34), (Key)(column + 34) }, ModifierKeys.Alt));
        //this.InputBindings.Add(hotKey);
      }
    }
  }
}