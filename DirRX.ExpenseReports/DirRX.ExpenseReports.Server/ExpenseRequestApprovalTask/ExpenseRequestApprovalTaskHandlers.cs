using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestApprovalTask;

namespace DirRX.ExpenseReports
{
  partial class ExpenseRequestApprovalTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {

    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.ExpenseRequestApprovalTask.UpdateTaskSubject(_obj);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Employee = Sungero.Company.Employees.As(_obj.Author);
      
      // Заполнить Дату отчета по-умолчанию
      _obj.ExpenseReportDate = Calendar.AddWorkingDays(Calendar.Today, 3);
      
      if (!_obj.State.IsCopied)
        _obj.ExpenseSum = 0;
    }
  }

}