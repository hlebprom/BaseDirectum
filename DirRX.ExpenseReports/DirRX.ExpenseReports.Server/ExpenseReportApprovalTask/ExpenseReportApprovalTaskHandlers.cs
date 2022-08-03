using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportApprovalTask;

namespace DirRX.ExpenseReports
{
  partial class ExpenseReportApprovalTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = ExpenseReportApprovalTasks.Resources.TaskBaseTheme;
      _obj.NeedPrepare = true;
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.Employee = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault().Employee;
    }
  }

}