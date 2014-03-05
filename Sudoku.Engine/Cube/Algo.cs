using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sudoku.Models;
using System.Collections.ObjectModel;

namespace Sudoku.Engine.Cube
{
  public class Algo : SudokuAlgoBase
  {
    public Algo(Board board) : base(board) { Initiate(); }

    #region Array Implementation
    //private List<int>[,] cube = null; // we can also create the 
    //private void Initiate()
    //{
    //  cube = new List<int>[Board.Size, Board.Size];

    //  foreach (var cell in Board)
    //    cube[cell.Row - 1, cell.Column - 1] = // arrary works on the basis of index
    //      cell.Square.Where(c => c.Value.HasValue).Select(c => c.Value.Value) // all square values
    //      .Union(cell.RowCells.Where(c => c.Value.HasValue).Select(c => c.Value.Value)) // all cell valu
    //      .Union(cell.ColumnCells.Where(c => c.Value.HasValue).Select(c => c.Value.Value))
    //      .Distinct()
    //      .ToList();
    //}
    #endregion

    private Dictionary<Cell, List<int>> cube = null; // we can also create the 
    private void Initiate()
    {
      cube = new Dictionary<Cell, List<int>>();
      foreach (var cell in Board)
        cube.Add(cell, null);

      Calculate();
    }

    private void Calculate()
    {
      foreach (var cell in Board)
        cube[cell] =
          cell.Square.Where(c => c.Value.HasValue).Select(c => c.Value.Value) // all square values
          .Union(cell.RowCells.Where(c => c.Value.HasValue).Select(c => c.Value.Value)) // all row values
          .Union(cell.ColumnCells.Where(c => c.Value.HasValue).Select(c => c.Value.Value)) // all column values
          .Distinct()
          .ToList();
    }

    public override string Name { get { return "Cube"; } }

    public override Tuple<Cell, int> Hint()
    {
      //Calculate(); // as we are overriding OnCellValueChange, we need not to calculate again
      var chosenCell = Board.Where<Cell>(cell => cell.Enabled).FirstOrDefault<Cell>(cell => cube[cell].Count == Board.Size - 1);
      if (chosenCell == null)
        return null;
      else
        return new Tuple<Cell, int>(chosenCell, GetRemaining(chosenCell).First()); // calculated remaining value using shortest path
    }

    protected override void OnCellValueChange(Cell cell, IEnumerable<Cell> affectedCells, int? previous, int? current)
    {
      //Calculate();

      if (!previous.HasValue && current.HasValue) // if the value is added
        affectedCells.ToList().ForEach(c => cube[c].AddDistinct(cell.Value.Value));
      else if (previous.HasValue && !current.HasValue) // if the value is removed
      {
        Calculate();
      }
      else if (previous.HasValue && current.HasValue && previous.Value != current.Value) // if the value is updated
      {
        Calculate();
      }
      // else // we do want any change in case previous and current values are same
    }

    public override Boolean IsValidMove(Cell cell)
    {
      if (cell.Value.HasValue)
      {
        var square = cell
          .Square
          .Where(c => !(c.Row == cell.Row && cell.Column == c.Column))
          .Where(c => c.Value.HasValue)
          .Select(c => c.Value.Value);
        var row = cell
          .RowCells
          .Where(c => !(c.Row == cell.Row && cell.Column == c.Column))
          .Where(c => c.Value.HasValue)
          .Select(c => c.Value.Value);
        var column = cell
          .ColumnCells
          .Where(c => !(c.Row == cell.Row && cell.Column == c.Column))
          .Where(c => c.Value.HasValue)
          .Select(c => c.Value.Value);

        var filled = square.Union(row).Union(column);
        return Board.AllPossibleValues.Except(filled).Contains(cell.Value.Value);
      }
      else
        return false;
    }

    public IEnumerable<int> GetFilled(int row, int column)
    {
      return GetFilled(Board[row, column]);
    }

    public IEnumerable<int> GetRemaining(int row, int column)
    {
      return Board.AllPossibleValues.Except(GetFilled(row, column));
    }

    public IEnumerable<int> GetFilled(Cell cell)
    {
      return cube[cell].AsEnumerable<int>();
    }

    public IEnumerable<int> GetRemaining(Cell cell)
    {
      return Board.AllPossibleValues.Except(cube[cell]);
    }
  }
}