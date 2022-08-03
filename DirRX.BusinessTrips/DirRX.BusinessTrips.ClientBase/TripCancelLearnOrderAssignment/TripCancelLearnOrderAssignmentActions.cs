using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.TripCancelLearnOrderAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class TripCancelLearnOrderAssignmentActions
  {
    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var order = _obj.CancelOrderGroup.BusinessTripOrders.FirstOrDefault();
      DirRX.ExpenseReports.PublicFunctions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), order, e.Action.LocalizedName, e, true);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}