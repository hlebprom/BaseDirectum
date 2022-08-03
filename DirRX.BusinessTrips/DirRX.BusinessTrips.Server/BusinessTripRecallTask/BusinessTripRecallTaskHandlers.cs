using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripRecallTask;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripRecallTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.OtherGroup.All.Add(_obj.BusinessTrip);
      _obj.OtherGroup.All.Add(Functions.BusinessTrip.GetBusinessTripApprovalTask(_obj.BusinessTrip));
      
      _obj.CommonSubjectPart = BusinessTripRecallTasks.Resources.CommonSubjectPartTemplateFormat(_obj.BusinessTrip.Employee.Name,
                                                                                                 _obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                 _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                 _obj.BusinessTrip.Purpose);
      
      _obj.Subject = BusinessTripRecallTasks.Resources.TaskSubjectTemplateFormat(_obj.CommonSubjectPart);
      _obj.ActiveText = BusinessTripRecallTasks.Resources.TaskTextTemplateFormat(_obj.RecallReason,
                                                                                 _obj.RecallDate.Value.ToString("d"));
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = BusinessTripRecallTasks.Resources.TaskThemeBase;
    }
  }

}