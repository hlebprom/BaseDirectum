using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalTask;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripApprovalTaskServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.BusinessTripApprovalTask.UpdateTaskSubject(_obj);
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      foreach(var routeStep in _obj.Route)
      {
        // Дата начала командировки должна быть меньше или равна дате прибытия в пункт назначения.
        if (routeStep.DateIn.Value < _obj.DepartureDate.Value)
          e.AddError(BusinessTripApprovalTasks.Resources.TripDepartureDateTooLateError);
        
        // Дата окончания командировки должна быть больше или равна дате отъезда из пункта назначения.
        if (routeStep.DateOut.Value > _obj.ReturnDate.Value)
          e.AddError(BusinessTripApprovalTasks.Resources.TripReturnDateTooEarlyError);        
      }

      // Сохранить текст в параметр, чтобы не потерялся при переформировании
      _obj.StartTaskText = _obj.ActiveText;

      Functions.BusinessTripApprovalTask.UpdateTaskText(_obj);
      
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = BusinessTripApprovalTasks.Resources.TaskThemeBase;
      _obj.Employee = Sungero.Company.Employees.As(_obj.Author);
      _obj.ByCar = false;
      _obj.NeedTicketAndHotel = false;
      _obj.MoneyTransferred = false;
      _obj.IsChangedByUser = false;
      _obj.ChangeOrderExists = false;
      _obj.NeedChange = false;
      _obj.TicketAndHotelProccessed = false;
      _obj.OrderSigned = false;
      _obj.OldExpensesSum = 0;
      
      // Добавить инициатора в список для уведомления в случает отмены
      Functions.BusinessTripApprovalTask.AddRecipientForAbortNotification(_obj, _obj.Author);
    }
  }

}