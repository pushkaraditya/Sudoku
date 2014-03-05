using System;
using System.Linq;
using Sudoku.Models;
using System.Collections.Generic;

namespace Sudoku.Engine
{
  public abstract class SudokuAlgoBase : ISudokuAlgo
  {
    public SudokuAlgoBase(Board board)
    {
      Board = board;
      Cell.CellValueChanged += OnCellValueChange;
    }

    // not sure if this method has to be marked abstract as base level, there cannot be any implementation
    // however making it not abstract will certain algo which do not want to take advantage of this method
    protected virtual void OnCellValueChange(Cell cell, IEnumerable<Cell> affectedCells, int? previous, int? current)
    {
    }

    public Board Board { get; private set; }

    public abstract string Name { get; }

    public abstract Tuple<Cell, int> Hint();

    public Board Solve()
    {
      var hint = Hint();
      while (hint != null)
      {
        hint.Item1.Value = hint.Item2;
        hint = Hint();
      }
      if (Board.IsSolved())
        return Board;
      else
        throw new Exception(string.Format("Board cannot be solved completely using {0} algo", Name));
    }

    public abstract Boolean IsValidMove(Cell cell);

    // These are the methods which probably can be used in every Algo
    // however for validating setup we might be needing all possible
    // algos to ensure that the set up has only one and one solution
    #region BaseClassMethods

    public void ValidateSetup()
    {
      ValidateBasicRules();
    }

    public string ValidateBasicRules()
    {
      var error = string.Empty;

      var invalidSquares = Board.Squares.Where(square => !square.IsValid());
      if (invalidSquares.Count() > 0)
        error += string.Format("Invalid Squares: {0}", string.Join(";", invalidSquares.Select(square => string.Format("{0},{1}", square.Row1, square.Column1))));

      var invalidRows = from row in Enumerable.Range(1, Board.Size)
                        where !Board.GetRow(row).IsValid()
                        select row;
      if (invalidRows.Count() > 0)
        error += string.Format("Invalid Rows: {0}", string.Join<int>(";", invalidRows));

      var invalidColumns = from column in Enumerable.Range(1, Board.Size)
                           where !Board.GetColumn(column).IsValid()
                           select column;
      if (invalidColumns.Count() > 0)
        error += string.Format("Invalid Columns: {0}", string.Join<int>(";", invalidColumns));

      return error;
    }

    protected Boolean CheckBasicMoveValidation(Cell cell)
    {
      return true;
    }

    #endregion
  }
}