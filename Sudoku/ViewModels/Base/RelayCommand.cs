using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Sudoku.ViewModels.Base
{
  public class RelayCommand : ICommand
  {
    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public RelayCommand(Action<object> execute) : this(execute, p => true) { }
    public RelayCommand(Action execute) : this(p => execute()) { }
    public RelayCommand(Action execute, Func<bool> canExecute) : this(p => execute(), p => canExecute()) { }

    private Action<object> execute = null;
    private Predicate<object> canExecute = null;

    public bool CanExecute(object parameter)
    {
      return canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
      execute(parameter);
    }

    public void UpdateCanExecuteChanged()
    {
      try
      {
        if (CanExecuteChanged != null)
          CanExecuteChanged(this, new EventArgs());
      }
      catch
      {
        // I am going to die!! Async thread are causing problems and I am not sure how can I solve it, please don't suggest backgroud worker ROC (returns on Code written) is not that high :'(
      }
    }
  }
}