using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReport;

namespace DirRX.ExpenseReports.Shared
{
  partial class ExpenseReportFunctions
  {
    /// <summary>
    /// Проверить и подсветить превышение лимита по строке расхода.
    /// </summary>
    /// <param name="expenseRow">Строка таблицы расходов.</param>
    public void CheckLimit(IExpenseReportExpenses expenseRow)
    {
      if (expenseRow.ExpenseSum.HasValue && expenseRow.Limit.HasValue
          && expenseRow.ExpenseSum.Value > expenseRow.Limit.Value)
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
    /// Пересчитать количество документов и листов.
    /// </summary>
    public void ReCountDocsAndPages()
    {
      _obj.DocsCount = _obj.Expenses.Where(exp => exp.SupportingDoc != null).Select(exp => exp.SupportingDoc).Distinct().Count();
      _obj.PagesCount = _obj.Expenses.Where(exp => exp.SupportingDoc != null).Select(exp => exp.SupportingDoc).Distinct().Select(d => d.PagesCount).Sum();
    }
    
    /// <summary>
    /// Пересчитать суммы по авансовому отчету.
    /// </summary>
    public void ReCountSums()
    {
      if (_obj.Expenses.Any())
      {
        double expenseSum = 0;
        // Сложить все суммы расходов
        foreach(var expanseRow in _obj.Expenses)
        {
          if (expanseRow.ExpenseSum.HasValue)
            expenseSum += expanseRow.ExpenseSum.Value;
        }
        _obj.ApprovingSum = expenseSum;
      }
      else
      {
        // Если удалили все строки, то "Сумма к утверждению" = 0
        _obj.ApprovingSum = 0;
      }
      
      // Вычислить Остаток/Перерасход
      double gettedMoney = 0;
      if (_obj.GettedMoney.HasValue)
        gettedMoney = _obj.GettedMoney.Value;
      
      double expenses = 0;
      if (_obj.ApprovingSum.HasValue)
        expenses = _obj.ApprovingSum.Value;
      
      if (gettedMoney > expenses)
      {
        // Остаток = Получено - Израсходовано
        _obj.RemainMoney = gettedMoney - expenses;
        
        // Перерасход = 0
        _obj.OverrunMoney = 0;
      }
      else
      {
        // Остаток = 0
        _obj.RemainMoney = 0;
        
        // Перерасход = Израсходовано - Получено
        _obj.OverrunMoney = expenses - gettedMoney;
      }
    }
  }
}