using System.Windows.Controls;

namespace Sudoku.Controls.ValidationRules
{
  public class Range : ValidationRule
  {
    public Range(int min, int max)
    {
      Min = min;
      Max = max;
    }

    public int Min { get; private set; }
    public int Max { get; private set; }

    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
      int val;
      if (value != null && int.TryParse(value.ToString(), out val) && (Min > val || val > Max))
        return new ValidationResult(false, string.Format("Value should be between {0} and {1}", Min, Max));
      return ValidationResult.ValidResult;
    }
  }
}