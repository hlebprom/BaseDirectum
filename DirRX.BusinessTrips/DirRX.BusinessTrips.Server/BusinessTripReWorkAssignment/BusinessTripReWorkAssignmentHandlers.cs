using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripReWorkAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripReWorkAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
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
    }
  }



}