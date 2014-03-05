using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sudoku.Models;
using System.Threading;
using System.Windows.Input;

namespace Sudoku.ViewModels
{
  /// <summary>
  /// Only Responsibility of this class to help taking undo and redo actions
  /// </summary>
  public class Recorder
  {
    #region Singleton
    private Recorder() { }

    private static object guard = new object();
    private static Recorder instance = null;
    public static Recorder Manager
    {
      get
      {
        if (instance == null)
          lock (guard)
            if (instance == null)
            {
              instance = new Recorder();
              Cell.CellValueChanged += instance.RecordChange;
            }
        return instance;
      }
    }

    #endregion

    public Board Board { get; private set; }
    private bool isSetup = false;
    private volatile bool isRecording = false;
    private volatile bool isCounterInvalid = false;
    private volatile int counter = 0;
    private Dictionary<int, Record> records = new Dictionary<int, Record>();
    private void RecordChange(Cell cell, IEnumerable<Cell> affectedCells, int? previous, int? current)
    {
      if (isSetup && !isRecording)
      {
        ++counter;
        if (isCounterInvalid)
        {
          var max = records.Keys.Max();
          for (int key = counter; key <= max; key++)
            if (records.ContainsKey(key))
              records.Remove(key);
          isCounterInvalid = false;
        }
        records.Add(counter, new Record(cell, previous, current));
      }
    }

    public void SetupBoard(Board board)
    {
      if (isSetup)
        throw new InvalidOperationException("Board is already setup");
      else
      {
        Board = board;
        isSetup = true;
      }
    }

    private void ValidateOperation()
    {
      if (!isSetup)
        throw new InvalidOperationException("Board is not setup. Call SetupBoard method to setup");
    }

    public void Undo()
    {
      if (CanUndo())
      {
        isCounterInvalid = true;
        isRecording = true;
        var record = records[counter];
        //records.Remove(counter);
        --counter;
        record.Cell.Value = record.Previous;
        isRecording = false;
      }
    }

    public bool CanUndo()
    {
      ValidateOperation();
      return counter > 0;
    }

    public void ReDo()
    {
      if (CanReDo())
      {
        isCounterInvalid = true;
        isRecording = true;
        var record = records[++counter];
        //records.Add(counter);
        record.Cell.Value = record.Current;
        isRecording = false;
      }
    }

    public bool CanReDo()
    {
      ValidateOperation();
      return isCounterInvalid && counter < records.Keys.Max();
    }

    public void Clear(int delay = 0)
    {
      ValidateOperation();
      isRecording = true;
      while (counter > 0)
      {
        var record = records[counter];
        records.Remove(counter);
        --counter;
        record.Cell.Value = record.Previous;
        Thread.Sleep(delay);
      }
      isRecording = false;
    }

    public void ClearAsync()
    {
      Action<int> clear = Clear;
      clear.BeginInvoke(85, null, null);
    }

    private class Record
    {
      public Record(Cell cell, int? previous, int? current)
      {
        Cell = cell;
        Previous = previous;
        Current = current;
      }
      public Cell Cell { get; private set; }
      public int? Previous { get; private set; }
      public int? Current { get; private set; }
    }
  }
}