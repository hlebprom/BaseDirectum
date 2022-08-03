using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripPrepareReportAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripPrepareReportAssignmentExpensesSupportingDocPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ExpensesSupportingDocFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      // Для выбора отображать только документы:
      // 1) Пользователь = Автор
      // 2) Ещё не выбирался в других Авансовых отчетах
      return query.Where(d => d.Author.Equals(Sungero.Company.Employees.Current) &&
                         !DirRX.ExpenseReports.ExpenseReports.GetAll().Any(r => !r.Equals(_obj.BusinessTripPrepareReportAssignment.ExpenseReportGroup.ExpenseReports.FirstOrDefault()) &&
                                                                           r.Expenses.Any(exp => exp.SupportingDoc.Equals(d))
                                                                          )
                        );
    }
  }

  partial class BusinessTripPrepareReportAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      // Проверить даты только если задание выполняет пользователь. При Изменении командировки проверка факт. дат не требудется.
      if (_obj.Result.Value == Result.Complete)
      {
        foreach(var routeStep in _obj.Route)
        {
          // HACK проверка вида "!routeStep.FactDateIn.HasValue" нужна для обхода бага платформы, которая не отрабатывает незаполненные обязательные поля талицы
          // Факт. дата начала командировки должна быть меньше или равна факт. дате прибытия в пункт назначения.
          if (!routeStep.FactDateIn.HasValue || routeStep.FactDateIn.Value < _obj.FactDepartureDate.Value)
            e.AddError(BusinessTripPrepareReportAssignments.Resources.TripFactDepartureDateTooLateError);
          
          // Факт. дата окончания командировки должна быть больше или равна факт. дате отъезда из пункта назначения.
          if (!routeStep.FactDateOut.HasValue || routeStep.FactDateOut.Value > _obj.FactReturnDate.Value)
            e.AddError(BusinessTripPrepareReportAssignments.Resources.TripFactReturnDateTooEarlyError);
        }
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.PagesCount = 0;
      _obj.DocsCount = 0;
      
      _obj.GettedMoney = 0;
      _obj.ApprovingSum = 0;
    }
  }

}