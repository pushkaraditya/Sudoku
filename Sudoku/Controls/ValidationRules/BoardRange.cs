using Sudoku.Models;

namespace Sudoku.Controls.ValidationRules
{
  public class BoardRange : ValidationRules.Range
  {
    public BoardRange(Board board) : base(board.MinimumCellValue, board.MaximumCellValue) { }
  }
}