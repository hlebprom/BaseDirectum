using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestBossApprovalAssignment;

namespace DirRX.ExpenseReports
{
  partial class ExpenseRequestBossApprovalAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Подсветить превышение лимитов
      foreach(var expenseRow in _obj.Expenses)
      {
        if (expenseRow.ExpenseSum.HasValue && expenseRow.LimitSetting != null
            && expenseRow.LimitSetting.Limit.HasValue
            && expenseRow.ExpenseSum.Value > expenseRow.LimitSetting.Limit.Value)
        {
          expenseRow.State.Properties.ExpenseSum.HighlightColor = Sungero.Core.Colors.Parse(Constants.Module.LimitHighlightColorRed);
          expenseRow.State.Properties.LimitSetting.HighlightColor = Sungero.Core.Colors.Parse(Constants.Module.LimitHighlightColorRed);
        }
      }
    }

  }
}