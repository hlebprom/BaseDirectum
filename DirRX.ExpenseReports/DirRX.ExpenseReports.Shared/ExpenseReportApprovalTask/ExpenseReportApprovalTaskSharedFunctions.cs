using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportApprovalTask;

namespace DirRX.ExpenseReports.Shared
{
  partial class ExpenseReportApprovalTaskFunctions
  {

    /// <summary>
    /// Обновить тему задачи.
    /// </summary>
    public void UpdateTaskSubject()
    {
      var expenseReport = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      if (expenseReport != null)
        _obj.CommonSubjectPart = ExpenseReportApprovalTasks.Resources.TaskCommonSubjectPartTemplateFormat(expenseReport.RegistrationNumber ?? String.Empty,
                                                                                                          expenseReport.RegistrationDate.HasValue ? expenseReport.RegistrationDate.Value.ToString("d") : String.Empty,
                                                                                                          expenseReport.Employee.Name,
                                                                                                          expenseReport.Purpose);
      else
        _obj.CommonSubjectPart = string.Empty;
      
      _obj.Subject = ExpenseReportApprovalTasks.Resources.TaskSubjectTemplateFormat(_obj.CommonSubjectPart);
    }

    /// <summary>
    /// Вложить подтверждающие документы по авансовому отчету.
    /// </summary>
    /// <remarks>Если нет Авансового отчета, подтверждающие документы удалятся.</remarks>
    public void AttachSupportingDocs()
    {
      var expRep = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      foreach (var doc in _obj.OtherGroup.All)
      {
        if (SupportingDocuments.Is(doc))
          _obj.OtherGroup.All.Remove(doc);
      }
      if (expRep != null)
      {
        var docs = expRep.Expenses.Where(exp => exp.SupportingDoc != null).Select(exp => exp.SupportingDoc).Distinct();
        foreach (var doc in docs)
          _obj.OtherGroup.All.Add(doc);
        
      }
    }

  }
}