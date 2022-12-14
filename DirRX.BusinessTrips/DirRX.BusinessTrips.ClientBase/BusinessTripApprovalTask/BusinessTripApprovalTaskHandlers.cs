using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalTask;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripApprovalTaskExpensesClientHandlers
  {
    
    public virtual void ExpensesExpenseSumValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(BusinessTripApprovalTasks.Resources.SubzeroError);
    }
  }

  partial class BusinessTripApprovalTaskRouteClientHandlers
  {

    public virtual void RouteDateOutValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Дата прибытия не может быть больше даты отъезда.
      if (e.NewValue.HasValue && _obj.DateIn.HasValue
          && e.NewValue.Value < _obj.DateIn.Value)
        e.AddError(BusinessTripApprovalTasks.Resources.RouteDateInMoreThenDateOutError);
    }

    public virtual void RouteDateInValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Дата прибытия не может быть больше даты отъезда.
      if (e.NewValue.HasValue && _obj.DateOut.HasValue
          && e.NewValue.Value > _obj.DateOut.Value)
        e.AddError(BusinessTripApprovalTasks.Resources.RouteDateInMoreThenDateOutError);
    }
  }

  partial class BusinessTripApprovalTaskClientHandlers
  {

    public virtual void ByCarValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue.Value)
        e.AddInformation(BusinessTripApprovalTasks.Resources.ByCarMemoInfo);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.OrderGroup.IsVisible = _obj.OrderGroup.All.Any();
      _obj.State.Attachments.BusinessTripGroup.IsVisible = _obj.BusinessTripGroup.All.Any();
      _obj.State.Attachments.ExpenseReportGroup.IsVisible = _obj.ExpenseReportGroup.All.Any();
      
      foreach(var expenseRow in _obj.Expenses)
      {
        // Подсветить превышение лимитов
        Functions.BusinessTripApprovalTask.CheckLimit(_obj, expenseRow);
      }
    }

    public virtual void ReturnDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Дата возвращения должна быть больше или равна дате отъезда.
      if (_obj.DepartureDate.HasValue && e.NewValue.HasValue && e.NewValue < _obj.DepartureDate)
        e.AddError(BusinessTripApprovalTasks.Resources.TripDatesErrorText);
    }

    public virtual void DepartureDateValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      // Дата возвращения должна быть больше или равна дате отъезда.
      if (_obj.ReturnDate.HasValue && e.NewValue.HasValue && _obj.ReturnDate < e.NewValue)
        e.AddError(BusinessTripApprovalTasks.Resources.TripDatesErrorText);
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      Functions.BusinessTripApprovalTask.ProcessCarProperties(_obj);
      
      if (_obj.Expenses.Where(ex => ex.Limit.HasValue && ex.ExpenseSum.HasValue && ex.ExpenseSum.Value > ex.Limit.Value).Any())
        e.AddInformation(DirRX.ExpenseReports.ExpenseReports.Resources.LimitExceededErrorText);
    }

  }
}