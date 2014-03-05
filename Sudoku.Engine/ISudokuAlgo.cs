using System;
using Sudoku.Models;

namespace Sudoku.Engine
{
  public interface ISudokuAlgo
  {
    string Name { get; }

    /// <summary>
    /// This method can return null value if algo is not able to find any value
    /// </summary>
    /// <returns></returns>
    Tuple<Cell, int> Hint();

    Boolean IsValidMove(Cell cell);

    Board Solve();
  }
}