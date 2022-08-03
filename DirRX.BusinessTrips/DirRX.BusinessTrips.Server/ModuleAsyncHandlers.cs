using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.BusinessTrips.Server
{
  public class ModuleAsyncHandlers
  {

    public virtual void ChangeBusinessTrip(DirRX.BusinessTrips.Server.AsyncHandlerInvokeArgs.ChangeBusinessTripInvokeArgs args)
    {
      var task = BusinessTripApprovalTasks.GetAll(t => t.Id == args.TaskID).FirstOrDefault();
      
      var assignments = Sungero.Workflow.Assignments.GetAll(a => Equals(task, a.Task) && a.Result == null);
      if (assignments.Any())
      {
        foreach (var assignment in assignments)
        {
          if (DirRX.BusinessTrips.BusinessTripApprovalAssignments.Is(assignment))
            assignment.Complete(DirRX.BusinessTrips.BusinessTripApprovalAssignment.Result.ForceReWork);
          else if (DirRX.BusinessTrips.BusinessTripSignOrderAssignments.Is(assignment))
            assignment.Complete(DirRX.BusinessTrips.BusinessTripSignOrderAssignment.Result.ForceReWork);
          else if (DirRX.BusinessTrips.BusinessTripSimpleAssignments.Is(assignment))
            assignment.Complete(DirRX.BusinessTrips.BusinessTripSimpleAssignment.Result.ForceReWork);
          else if (DirRX.BusinessTrips.BusinessTripProcessDocumentsAssignments.Is(assignment))
            assignment.Complete(DirRX.BusinessTrips.BusinessTripProcessDocumentsAssignment.Result.ForceReWork);
          else if (DirRX.BusinessTrips.BusinessTripPrepareReportAssignments.Is(assignment))
            assignment.Complete(DirRX.BusinessTrips.BusinessTripPrepareReportAssignment.Result.ForceReWork);
        }
      }
    }
  }
}