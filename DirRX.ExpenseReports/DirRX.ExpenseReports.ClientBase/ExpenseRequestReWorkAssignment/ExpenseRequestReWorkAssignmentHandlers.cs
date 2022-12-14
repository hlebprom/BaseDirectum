using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestReWorkAssignment;

namespace DirRX.ExpenseReports
{
  partial class ExpenseRequestReWorkAssignmentExpensesClientHandlers
  {

    public virtual void ExpensesExpenseSumValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value <= 0)
        e.AddError(ExpenseRequestApprovalTasks.Resources.SubzeroError);
    }
  }



  partial class ExpenseRequestReWorkAssignmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      if (_obj.Expenses.Where(ex => ex.LimitSetting != null && ex.LimitSetting.Limit.HasValue && ex.ExpenseSum.HasValue && ex.ExpenseSum.Value > ex.LimitSetting.Limit.Value).Any())
        e.AddInformation(DirRX.ExpenseReports.ExpenseReports.Resources.LimitExceededErrorText);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      foreach(var expenseRow in _obj.Expenses)
      {
        // Подсветить превышение лимитов
        Functions.ExpenseRequestReWorkAssignment.CheckLimit(_obj, expenseRow);
      }
    }

    public virtual void ExpenseReportDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < Calendar.Today)
        e.AddError(ExpenseRequestApprovalTasks.Resources.DateError);
    }

    public virtual void ExpenseSumValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value <= 0)
        e.AddError(ExpenseRequestApprovalTasks.Resources.SubzeroError);
    }

  }
}