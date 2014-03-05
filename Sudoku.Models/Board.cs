using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sudoku.Models
{
  public class Board : IEnumerable<Cell>, IEnumerable<Square> // by default it will be for cell
  {
    public readonly IEnumerable<int> AllPossibleValues = null;
    public Board(int squareSize)
    {
      SquareSize = squareSize;
      MaximumCellValue = Size = squareSize * squareSize;
      MinimumCellValue = 1;

      var range = Enumerable.Range(MinimumCellValue, MaximumCellValue);
      AllPossibleValues = Enumerable.Range(MinimumCellValue, MaximumCellValue);

      Squares = new Squares(this);
    }

    public Board() : this(3) { }

    public int SquareSize { get; private set; }
    public int Size { get; private set; }

    public int MinimumCellValue { get; private set; }
    public int MaximumCellValue { get; private set; }

    public Squares Squares { get; private set; }

    public Cell this[int cellRow, int cellColumn]
    {
      get
      {
        this.ValidateIndexes(cellRow, cellColumn);
        return Squares[cellRow, cellColumn][cellRow, cellColumn];
      }
    }

    public IEnumerator<Cell> GetEnumerator()
    {
      foreach (var square in Squares)
        foreach (var cell in square)
          yield return cell;
    }

    IEnumerator<Square> IEnumerable<Square>.GetEnumerator()
    {
      return Squares.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerable<Cell> GetRow(int row)
    {
      return this.Where<Cell>(cell => cell.Row == row);
    }

    public IEnumerable<Cell> GetColumn(int column)
    {
      return this.Where<Cell>(cell => cell.Column == column);
    }
  }
}