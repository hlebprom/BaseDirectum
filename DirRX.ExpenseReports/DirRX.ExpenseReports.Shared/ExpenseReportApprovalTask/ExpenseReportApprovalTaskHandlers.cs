using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportApprovalTask;

namespace DirRX.ExpenseReports
{
  partial class ExpenseReportApprovalTaskSharedHandlers
  {

    public virtual void ExpenseReportGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      Functions.ExpenseReportApprovalTask.UpdateTaskSubject(_obj);
      Functions.ExpenseReportApprovalTask.AttachSupportingDocs(_obj);
      
    }

    public virtual void ExpenseReportGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      Functions.ExpenseReportApprovalTask.UpdateTaskSubject(_obj);
      Functions.ExpenseReportApprovalTask.AttachSupportingDocs(_obj);
    }

  }
}