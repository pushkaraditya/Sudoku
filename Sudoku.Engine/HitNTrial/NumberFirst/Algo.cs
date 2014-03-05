using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sudoku.Models;

namespace Sudoku.Engine.HitNTrial.NumberFirst
{
  public class Algo : SudokuAlgoBase
  {
    public Algo(Board board) : base(board) { }

    public override string Name { get { return "Number First Hit & Trial"; } }

    public override Tuple<Cell, int> Hint()
    {
      throw new NotImplementedException();
    }

    public override bool IsValidMove(Cell cell)
    {
      throw new NotImplementedException();
    }
  }
}