using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripPrepareReportAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripPrepareReportAssignmentExpensesClientHandlers
  {
    
    public virtual void ExpensesExpenseSumInCurrencyValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(BusinessTripApprovalTasks.Resources.SubzeroError);
    }

    public virtual void ExpensesExpenseSumValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(BusinessTripApprovalTasks.Resources.SubzeroError);
    }
  }

  partial class BusinessTripPrepareReportAssignmentRouteClientHandlers
  {

    public virtual void RouteFactDateOutValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Факт. дата прибытия не может быть больше факт. даты отъезда.
      if (e.NewValue.HasValue && _obj.FactDateIn.HasValue
          && e.NewValue.Value < _obj.FactDateIn.Value)
        e.AddError(BusinessTripPrepareReportAssignments.Resources.RouteFactDateInMoreThenFactDateOutError);
    }

    public virtual void RouteFactDateInValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Факт. дата прибытия не может быть больше факт. даты отъезда.
      if (e.NewValue.HasValue && _obj.FactDateOut.HasValue
          && e.NewValue.Value > _obj.FactDateOut.Value)
        e.AddError(BusinessTripPrepareReportAssignments.Resources.RouteFactDateInMoreThenFactDateOutError);
    }
  }

  partial class BusinessTripPrepareReportAssignmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      if (_obj.Expenses.Where(ex => ex.Limit.HasValue && ex.ExpenseSum.HasValue && ex.ExpenseSum.Value > ex.Limit.Value).Any())
        e.AddInformation(DirRX.ExpenseReports.ExpenseReports.Resources.LimitExceededErrorText);
    }

    public virtual void PagesCountValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(BusinessTripApprovalTasks.Resources.SubzeroError);
    }

    public virtual void DocsCountValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(BusinessTripApprovalTasks.Resources.SubzeroError);
    }

    public virtual void FactReturnDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Проверить корректность введённых дат
      if (_obj.FactDepartureDate.HasValue && e.NewValue.HasValue && _obj.FactDepartureDate > e.NewValue)
      {
        // Факт. дата возвращения должна быть больше или равна факт. дате отъезда.
        e.AddError(BusinessTripPrepareReportAssignments.Resources.TripDatesErrorText);
      }
    }

    public virtual void FactDepartureDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Проверить корректность введённых дат
      if (_obj.FactReturnDate.HasValue && e.NewValue.HasValue && _obj.FactReturnDate < e.NewValue)
      {
        // Факт. дата возвращения должна быть больше или равна факт. дате отъезда.
        e.AddError(BusinessTripPrepareReportAssignments.Resources.TripDatesErrorText);
      }
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      if (_obj.Status == Status.InProcess)
      {
        _obj.State.Properties.Route.Properties.FactDateIn.IsEnabled = true;
        _obj.State.Properties.Route.Properties.FactDateOut.IsEnabled = true;
        
        // Обязательность факт дат ставим в коде, т.к. изначально поля пустые
        _obj.State.Properties.Route.Properties.FactDateIn.IsRequired = true;
        _obj.State.Properties.Route.Properties.FactDateOut.IsRequired = true;
        
        _obj.State.Properties.FactDepartureDate.IsRequired = true;
        _obj.State.Properties.FactReturnDate.IsRequired = true;
      }
      
      foreach(var expenseRow in _obj.Expenses)
      {
        // Подсветить превышение лимитов
        Functions.BusinessTripPrepareReportAssignment.CheckLimit(_obj, expenseRow);
      }
      
      // Скрыть авансовый отчет из вложений, чтобы у пользователя не было желания заполнять его, а не Расходы на второй вкладке.
      _obj.State.Attachments.ExpenseReportGroup.IsVisible = false;
      
      // Таблицу расходов сделать обязательной при показе, т.к. изначально она пустая
      _obj.State.Properties.Expenses.IsRequired = true;
    }

  }
}