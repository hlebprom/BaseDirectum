using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripChangeOrderTask;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripChangeOrderTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      _obj.BusinessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj.BusinessTripApprovalTask);
      _obj.OtherGroup.All.Add(_obj.BusinessTrip);
      _obj.OtherGroup.All.Add(_obj.BusinessTripApprovalTask);

      _obj.Subject = BusinessTripChangeOrderTasks.Resources.TaskSubjectTemplateFormat(_obj.BusinessTrip.Employee.Name,
                                                                                      _obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                      _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                      _obj.BusinessTrip.Purpose);
      
      _obj.ActiveText = BusinessTripChangeOrderTasks.Resources.TaskTextTemplateFormat(_obj.ChangeReason);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = BusinessTripChangeOrderTasks.Resources.TaskThemeBase;
    }
  }

}