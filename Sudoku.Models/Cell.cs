using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Sudoku.Models
{
  [DebuggerDisplay("Row: {Row} Column: {Column} Value: {Value}")]
  public class Cell : Base.ViewModel, IDataErrorInfo
  {
    public const bool ThrowExceptionIfValueChangedInCaseOfLock = true;

    public Cell(Board board, Square square, int row, int column)
    {
      Board = board;
      Square = square;
      Row = row;
      Column = column;
      Enabled = true;
    }

    public static event CellValueChangedDelegate CellValueChanged;

    public Board Board { get; private set; }
    public Square Square { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    public bool Enabled { get; private set; }

    private int? val;
    /// <summary>
    /// We can write Set value logic here but this seems prettier this way, nonetheless helping in method chaining
    /// Buhuhu .. subuk .. subuk
    /// For WPF to work, set cannot be private
    /// </summary>
    public int? Value
    {
      get
      {
        return val;
      }
      set
      {
        SetValue(value);
      }
    }

    public Cell Lock()
    {
      Enabled = false;
      OnPropertyChange("Enabled");
      return this; // I don't think I am going to use this return statement :'((
    }

    /// <summary>
    /// Done for using method chaining :D
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Cell SetValue(int? value)
    {
      if (Enabled)
      {
        if (this.IsValid(value)) // I seems to be marveling my excellence here, a perfect valid sentence in code which can be read by lit grad
        {
          var oldValue = val;
          val = value;
          Error = null;
          OnPropertyChange("Value");
          OnCellValueChanged(oldValue, val);
        }
        else
        {
          Error = string.Format("cell value should be between {0} and {1}", Board.MinimumCellValue, Board.MaximumCellValue);
          throw new ArgumentOutOfRangeException("Value", value, Error);
        }
      }
      else if (ThrowExceptionIfValueChangedInCaseOfLock)
        throw new InvalidOperationException("Value cannot be changed when locked");
      return this;
    }

    protected void OnCellValueChanged(int? previous, int? current)
    {
      if (CellValueChanged != null)
      {
        var affectedCells = Square.Union(RowCells).Union(ColumnCells).Distinct();
        CellValueChanged(this, affectedCells, previous, current);
      }
    }

    public string Error { get; private set; }

    public string this[string columnName]
    {
      get { return Error; }
    }

    public IEnumerable<Cell> RowCells
    {
      get
      {
        return Board.Where<Cell>(cell => cell.Row == this.Row);
      }
    }

    public IEnumerable<Cell> ColumnCells
    {
      get
      {
        return Board.Where<Cell>(cell => cell.Column == this.Column);
      }
    }
  }
  public delegate void CellValueChangedDelegate(Cell cell, IEnumerable<Cell> affectedCells, int? previous, int? current);
}