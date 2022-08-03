using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestReWorkAssignment;

namespace DirRX.ExpenseReports.Shared
{
  partial class ExpenseRequestReWorkAssignmentFunctions
  {
    /// <summary>
    /// Проверить и подсветить превышение лимита по строке расхода.
    /// </summary>
    /// <param name="expenseRow">Строка таблицы расходов.</param>
    public void CheckLimit(IExpenseRequestReWorkAssignmentExpenses expenseRow)
    {
      if (expenseRow.ExpenseSum.HasValue && expenseRow.LimitSetting != null
          && expenseRow.LimitSetting.Limit.HasValue
          && expenseRow.ExpenseSum.Value > expenseRow.LimitSetting.Limit.Value)
      {
        expenseRow.State.Properties.ExpenseSum.HighlightColor = Sungero.Core.Colors.Parse(Constants.Module.LimitHighlightColorRed);
        expenseRow.State.Properties.LimitSetting.HighlightColor = Sungero.Core.Colors.Parse(Constants.Module.LimitHighlightColorRed);
      }
      else
      {
        expenseRow.State.Properties.ExpenseSum.HighlightColor = Colors.Empty;
        expenseRow.State.Properties.LimitSetting.HighlightColor = Colors.Empty;
      }
    }
    
    /// <summary>
    /// Пересчитать сумму расходов.
    /// </summary>
    public void ReCountSum()
    {
      _obj.ExpenseSum = 0;
      // Сложить все суммы расходов
      foreach(var expanseRow in _obj.Expenses)
      {
        if (expanseRow.ExpenseSum.HasValue)
          _obj.ExpenseSum += expanseRow.ExpenseSum.Value;
      }
    }
  }
}