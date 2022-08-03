using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportEmployeeSignAssignment;

namespace DirRX.ExpenseReports.Client
{
  partial class ExpenseReportEmployeeSignAssignmentActions
  {
    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var document = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      if (!document.Versions.Any())
      {
        e.AddError(ExpenseReportEmployeeSignAssignments.Resources.NoVersionErrorText);
        return;
      }
      
      // Подписать
      Functions.Module.ApproveDocument(_obj, document, _obj.ActiveText, e, true);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}