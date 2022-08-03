using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestApprovalTask;

namespace DirRX.ExpenseReports.Server
{
  partial class ExpenseRequestApprovalTaskFunctions
  {
    /// <summary>
    /// Обновить тему задачи
    /// </summary>
    [Remote]
    public void UpdateTaskSubject()
    {
      _obj.CommonSubjectPart = ExpenseRequestApprovalTasks.Resources.CommonSubjectPartTemplateFormat(_obj.ExpenseSum, _obj.Employee.Name, _obj.Purpose);
      _obj.Subject = ExpenseRequestApprovalTasks.Resources.TaskSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
  }
}