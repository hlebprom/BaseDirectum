using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripCancelTask;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripCancelTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = BusinessTripCancelTasks.Resources.TaskThemeBase;
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.BusinessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj.BusinessTripApprovalTask);
      _obj.OtherGroup.All.Add(_obj.BusinessTrip);
      _obj.OtherGroup.All.Add(_obj.BusinessTripApprovalTask);
      
      _obj.CommonSubjectPartNoFIO = BusinessTripCancelTasks.Resources.CommonSubjectPartNoFIOTemplateFormat(_obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                           _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                           _obj.BusinessTrip.Purpose);
      _obj.CommonSubjectPart = BusinessTripCancelTasks.Resources.CommonSubjectPartTemplateFormat(_obj.BusinessTrip.Employee.Name,
                                                                                                 _obj.CommonSubjectPartNoFIO);
      
      _obj.Subject = BusinessTripCancelTasks.Resources.TaskSubjectTemplateFormat(_obj.CommonSubjectPart);
      
      _obj.ActiveText = BusinessTripCancelTasks.Resources.CancelReasonTemplateFormat(_obj.CancelReason);
      
    }
  }

}