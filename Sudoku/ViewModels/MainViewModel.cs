using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Sudoku.Models;
using Sudoku.Models.Base;
using Sudoku.ViewModels.Base;
using Sudoku;
using Sudoku.Controls;
using System.Collections.Generic;
using System.IO;

/* *
 * 1. We can help user by showing invalid move (2) Here rather than cube algo, it would be very helpful if we implement counter cube algo
 * 2. Need to test for other level of puzzles as well, we might need to implement other algorithms as well (1)
 * 3. We can show 3d cube to impress user (8)
 * 4. We can give button for clear and solve it for me (3)
 * 5. We should not override the solutions while exporting rather create more files in solution directory (4)
 * 6. We can give user an option to import puzzle (5)
 * 7. We can give user a timer functionality (7)
 * 8. We can manage the bookmarks for user and export them (6)
 * */


namespace Sudoku.ViewModels
{
  public class MainViewModel : ViewModel
  {
    public MainViewModel()
    {
      Exit = new RelayCommand(Application.Current.Shutdown);
      Export = new RelayCommand(ExportBoard);
      GiveMeHint = new RelayCommand(NotifyHint, () => !StartSolving);
      Clear = new RelayCommand(Recorder.Manager.ClearAsync);
      Undo = new RelayCommand(Recorder.Manager.Undo, Recorder.Manager.CanUndo);
      ReDo = new RelayCommand(Recorder.Manager.ReDo, Recorder.Manager.CanReDo);

#if DEBUG
      AnimateStart(0, 1, 10, (o) => Opacity = o, () => Notify("Puzzle have been setup"));
#else
      AnimateStart(0, 1, 4300, (o) => Opacity = o, () => Notify("Puzzle have been setup"));
#endif
      Notify("We are setting up a Sudoku puzzle\nTry to solve it within time");
      IEnumerable<List<int>> solns;
      Board = new Board()
        .SetBoard(GetValues(out solns));

      if (solns != null && solns.Count() > 0)
      {
        solns.ToList()
          .ForEach(cell => Board[cell[0], cell[1]].Value = cell[2]);
      }

      #region Instantiate Algos
      cubeAlgo = new Engine.Cube.Algo(this.Board);
      #endregion

      Cell.CellValueChanged += OnCellValueChanged;
      Recorder.Manager.SetupBoard(Board);
    }

    private void OnCellValueChanged(Cell cell, IEnumerable<Cell> affectedCells, int? previous, int? current)
    {
      //CommandManager.InvalidateRequerySuggested();
      Undo.UpdateCanExecuteChanged();
      ReDo.UpdateCanExecuteChanged();
      if (Board.IsComplete() && Board.IsValid())
        Notify("Puzzle solved successfully, congratulations! :)", 15000);
    }

    #region Algo Instances
    Engine.Cube.Algo cubeAlgo = null;
    #endregion

    public RelayCommand Exit { get; private set; }
    public RelayCommand Export { get; private set; }
    public RelayCommand GiveMeHint { get; private set; }
    public RelayCommand Clear { get; private set; }
    public RelayCommand Undo { get; private set; }
    public RelayCommand ReDo { get; private set; }

    private string message;
    public string Message
    {
      get { return message; }
      set
      {
        message = value;
        OnPropertyChange("Message");
      }
    }

    private double opacity = 0;
    public double Opacity
    {
      get { return opacity; }
      set
      {
        opacity = value;
        OnPropertyChange("Opacity");
      }
    }

    private bool onlyPuzzle = true;
    public bool OnlyPuzzle
    {
      get { return onlyPuzzle; }
      set
      {
        onlyPuzzle = value;
        OnPropertyChange("OnlyPuzzle");
      }
    }

    private bool startSolving = false;
    public bool StartSolving
    {
      get { return startSolving; }
      set
      {
        //CommandManager.InvalidateRequerySuggested(); // This does not seems to work :'(
        startSolving = value;
        OnPropertyChange("StartSolving");
        OnPropertyChange("GiveMeHint");
        if (startSolving)
          Solve();
      }
    }

    public Board Board { get; private set; }

    public void Notify(string message, int duration = 4000)
    {
      AnimateEnd(() => Message = message, () => Message = string.Empty, duration);
    }

    private void AnimateEnd(Action start, Action end, int duration)
    {
      Action action = () =>
      {
        Thread.Sleep(duration);
        end();
      };
      start();
      action.BeginInvoke(null, null);
    }

    private void AnimateStart(double start, double end, int duration, Action<double> intervalAction, Action handOff)
    {
      Action startAction = () =>
      {
        var steps = 100;
        var step = (end - start) / steps;
        var interval = duration / steps;
        for (int i = 0; i < steps; i++)
        {
          start += step;
          intervalAction(start);
          Thread.Sleep(interval);
        }
      };
      var result = startAction.BeginInvoke((r) => handOff(), null);
    }

    private int[][] GetValues(out IEnumerable<List<int>> solns)
    {
      int[][] values = null;
      solns = null;
      bool isSuccessful = false;
      if (File.Exists("complete.sudoku"))
      {
        try
        {
          Notify("Importing complete.sudoku");
          var lines = File.ReadAllLines("complete.sudoku");
          lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

          var brokenContent = lines.Select(line => line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList());
          values = brokenContent.Where(vals => bool.Parse(vals[vals.Count - 1]))
            .Select(vals => vals.Take(vals.Count - 1).Select(val => int.Parse(val)).ToArray())
            .ToArray();

          solns = brokenContent.Where(vals => !bool.Parse(vals[vals.Count - 1]))
            .Select(vals => vals.Take(vals.Count - 1).Select(val => int.Parse(val)).ToList());

          Notify("successfully imported complete.sudoku");
          isSuccessful = true;
        }
        catch
        {
          Notify("Failed importing complete.sudoku");
        }
      }

      if (!isSuccessful && File.Exists("puzzle.sudoku"))
      {
        try
        {
          Notify("Importing puzzle.sudoku");
          var lines = File.ReadAllLines("puzzle.sudoku");
          values = lines.Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(val => int.Parse(val)).ToArray())
            .ToArray();
          Notify("Successfully imported puzzle.sudoku");
          isSuccessful = true;
        }
        catch
        {
          Notify("Failed importing puzzle.sudoku");
        }
      }

      if (!isSuccessful)
      {
        values = new int[][]{

          new int[] { 1, 2, 7 },
          new int[] { 1, 8, 8 },

          new int[] { 2, 1, 5 },
          new int[] { 2, 4, 7 },
          new int[] { 2, 6, 6 },
          new int[] { 2, 9, 9 },

          new int[] { 3, 2, 6 },
          new int[] { 3, 3, 1 },
          new int[] { 3, 4, 8 },
          new int[] { 3, 6, 3 },
          new int[] { 3, 7, 7 },
          new int[] { 3, 8, 4 },

          new int[] { 4, 2, 2 },
          new int[] { 4, 5, 4 },
          new int[] { 4, 8, 1 },

          new int[] { 5, 1, 9 },
          new int[] { 5, 9, 8 },

          new int[] { 6, 2, 8 },
          new int[] { 6, 5, 6 },
          new int[] { 6, 8, 2 },

          new int[] { 7, 2, 3 },
          new int[] { 7, 3, 2 },
          new int[] { 7, 4, 5 },
          new int[] { 7, 6, 4 },
          new int[] { 7, 7, 1 },
          new int[] { 7, 8, 9 },

          new int[] { 8, 1, 7 },
          new int[] { 8, 4, 2 },
          new int[] { 8, 6, 9 },
          new int[] { 8, 9, 3 },

          new int[] { 9, 2, 9 },
          new int[] { 9, 8, 7 }
      };
      }
      return values;
    }

    private Board SetupBoard4()
    {
      return new Board(2);
    }

    private void ExportBoard()
    {
      var filename = OnlyPuzzle ? "puzzle.sudoku" : "complete.sudoku";
      IEnumerable<Cell> cells = null;
      if (OnlyPuzzle)
        cells = Board.Where<Cell>(cell => !cell.Enabled);
      else
        cells = Board;
      ExportCells(filename, cells);
    }

    private void ExportCells(string path, IEnumerable<Cell> cells)
    {
      if (cells == null)
        return;
      string format = OnlyPuzzle ? "{0},{1},{2}" : "{0},{1},{2},{3}";
      var lines = from cell in cells
                  where cell.Value.HasValue
                  orderby cell.Enabled
                  select string.Format(format, cell.Row, cell.Column, cell.Value.Value, !cell.Enabled);
      File.WriteAllLines(path, lines);
    }

    private void Solve()
    {
      Action cubeSolver = SolveUsingCubeAlgo;
      cubeSolver.BeginInvoke(null, null);
    }

    private void SolveUsingCubeAlgo()
    {
      var hint = cubeAlgo.Hint();
      while (hint != null)
      {
        if (!StartSolving)
          break;
        Thread.Sleep(500);
        hint.Item1.Value = hint.Item2;
        hint = cubeAlgo.Hint();
      }
    }

    private void NotifyHint()
    {
      var hint = cubeAlgo.Hint();
      if (hint == null)
        Notify("Existing Algos are not able to proceed, now you have to put your own brain to exercise", 10000);
      else
        Notify(string.Format("Row: {0} Column: {1} Value: {2}", hint.Item1.Row, hint.Item1.Column, hint.Item2), 6000);
    }
  }
}