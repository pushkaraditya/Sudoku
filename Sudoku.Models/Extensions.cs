using System;
using System.Linq;
using System.Collections.Generic;

namespace Sudoku.Models
{
  public static class Extensions
  {
    public static Board SetBoard(this Board board, params Tuple<int, int, int>[] values)
    {
      if (board == null)
        throw new ArgumentNullException("board");
      if (values == null)
        throw new ArgumentNullException("values");
      if (values.Any(val => val == null))
        throw new ArgumentNullException("values", "Any of the value is sent as null");

      values.ToList()
        .ForEach(value =>
          board[value.Item1, value.Item2]
          .SetValue(value.Item3)
          .Lock());

      return board;
    }

    public static Board SetBoard(this Board board, params int[][] values)
    {
      return board.SetBoard(values.ToList().ConvertAll(v => v.CreateTuple()).ToArray());
    }

    private static Tuple<int, int, int> CreateTuple(this int[] values)
    {
      return Tuple.Create<int, int, int>(values[0], values[1], values[2]);
    }

    public static void ValidateIndexes(this Board board, int row, int column)
    {
      ValidateIndexes(row, column, 1, board.Size, 1, board.Size);
    }

    internal static void ValidateIndexes(this Square square, int row, int column)
    {
      ValidateIndexes(row, column, square.Row1, square.Row2, square.Column1, square.Column2);
    }

    private static void ValidateIndexes(int row, int column, int minRow, int maxRow, int minColumn, int maxColumn)
    {
      if (row > maxRow)
        throw new ArgumentOutOfRangeException("row", row, string.Format("row cannot be greater than {0}", maxRow));
      if (column > maxColumn)
        throw new ArgumentOutOfRangeException("column", column, string.Format("y cannot be greater than {0}", maxColumn));
      if (row < minRow)
        throw new ArgumentOutOfRangeException("row", row, string.Format("row cannot be less than {0}", minRow));
      if (column < minColumn)
        throw new ArgumentOutOfRangeException("column", column, string.Format("column cannot be less {0}", minColumn));
    }

    public static bool IsValid(this Cell cell, int? value)
    {
      return ((value.HasValue && cell.Board.MinimumCellValue <= value && value <= cell.Board.MaximumCellValue)
        || !value.HasValue);
    }

    public static bool IsValid(this IEnumerable<Cell> cells)
    {
      var values = cells.Where(cell => cell.Value.HasValue).Select(cell => cell.Value.Value);
      return
        //cells.Where(cell => cell.Value.HasValue).Any(cell => !cell.Board.AllPossibleValues.Contains(cell.Value.Value)) && // we can safely ignores this check as we have put the validation and exception in place so this should never occur
        values.Count() == values.Distinct().Count();
    }

    public static bool IsValid(this Board board)
    {
      return !board.Squares.Any(square => !square.IsValid())
      && !Enumerable.Range(1, board.Size).Any(row => !board.GetRow(row).IsValid())
      && !Enumerable.Range(1, board.Size).Any(column => !board.GetColumn(column).IsValid());
    }

    public static bool IsComplete(this Board board)
    {
      return !board.Any<Cell>(cell => !cell.Value.HasValue);
    }

    public static bool IsSolved(this Board board)
    {
      return board.IsComplete() && board.IsValid();
    }

    public static void AddDistinct<T>(this List<T> list, T value)
    {
      if (!list.Contains(value))
        list.Add(value);
    }
  }
}