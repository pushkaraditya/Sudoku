using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Models
{
  public class Square : IEnumerable<Cell>
  {
    public Square(Board board, int squareRow, int squareColumn)
    {
      Board = board;

      Row1 = Board.SquareSize * (squareRow - 1) + 1; // 1 => 1, 2 => 4, 3 => 7
      Row2 = Board.SquareSize * squareRow; // 1 => 3, 2 => 6, 3 => 9

      Column1 = Board.SquareSize * (squareColumn - 1) + 1; // 1 => 1, 2 => 4, 3 => 7
      Column2 = Board.SquareSize * squareColumn; // 1 => 3, 2 => 6, 3 => 9

      cells = new List<Cell>();
      for (int row = Row1; row <= Row2; row++)
        for (int column = Column1; column <= Column2; column++)
          cells.Add(new Cell(Board, this, row, column));
    }

    public Board Board { get; private set; }

    public int Row1 { get; private set; }
    public int Row2 { get; private set; }
    public int Column1 { get; private set; }
    public int Column2 { get; private set; }

    private List<Cell> cells = null;

    public Cell this[int cellRow, int cellColumn]
    {
      get
      {
        this.ValidateIndexes(cellRow, cellColumn);
        return cells.Single(cell => cell.Row == cellRow && cell.Column == cellColumn);
      }
    }

    public IEnumerator<Cell> GetEnumerator()
    {
      foreach (var cell in cells)
      {
        yield return cell;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  public class Squares : IEnumerable<Square>
  {
    internal Squares(Board board)
    {
      Board = board;

      var squares = new List<Square>();
      for (int squareRow = 1; squareRow <= Board.SquareSize; squareRow++)
        for (int squareColumn = 1; squareColumn <= Board.SquareSize; squareColumn++)
        {
          squares.Add(new Square(Board, squareRow, squareColumn));
        }

      List = new List<Square>(squares); // there has to be some validations ensuring that sqares made are correct // marking constructor internal to escape this valiations
    }

    private List<Square> List { get; set; }
    public Board Board { get; private set; }

    public Square this[int cellRow, int cellColumn]
    {
      get
      {
        Board.ValidateIndexes(cellRow, cellColumn);
        return List.Single(square => square.Row1 <= cellRow && cellRow <= square.Row2 && square.Column1 <= cellColumn && cellColumn <= square.Column2);
      }
    }

    public IEnumerator<Square> GetEnumerator()
    {
      foreach (var square in List)
        yield return square;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}