using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportApprovalSignAssignment;

namespace DirRX.ExpenseReports.Client
{
  partial class ExpenseReportApprovalSignAssignmentActions
  {
    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(ExpenseReportApprovalSignAssignments.Resources.ReWorkNoActiveTextErrorText);
        return;
      }
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }


    public virtual void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      Functions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault(), e.Action.LocalizedName, e, true);
    }

    public virtual bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}