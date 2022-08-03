using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReport;
using Sungero.Company;

namespace DirRX.ExpenseReports
{

  partial class ExpenseReportExpensesSupportingDocPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ExpensesSupportingDocFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Для выбора отображать только документы: 
      // 1) Пользователь = Автор 
      // 2) Ещё не выбирался в других Авансовых отчетах
      return query.Where(d => d.Author.Equals(Employees.Current) && 
                         !ExpenseReports.GetAll().Any(r => !r.Equals(_obj.ExpenseReport) && 
                                                      r.Expenses.Any(exp => exp.SupportingDoc.Equals(d))
                                                     )
                        );
    }
  }

  partial class ExpenseReportServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      _obj.Subject = DirRX.ExpenseReports.ExpenseReports.Resources.ExpenseReportSubjectTemplateFormat(_obj.Employee.Name,
                                                                                                      _obj.Purpose);
      
      base.BeforeSave(e);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      // Предзаполнить свойства
      if (Sungero.Company.Employees.Current != null)
        _obj.Employee = Sungero.Company.Employees.Current;
      
      _obj.ExpenseReportStatus = ExpenseReport.ExpenseReportStatus.Start;
      _obj.PagesCount = 0;
      _obj.DocsCount = 0;
      
      _obj.GettedMoney = 0;
      _obj.ApprovingSum = 0;
      
      _obj.Subject = ExpenseReports.Resources.TempSubject;
    }
  }

}