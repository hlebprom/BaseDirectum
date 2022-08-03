using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.ChangeOrderSignAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class ChangeOrderSignAssignmentActions
  {
    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var order = _obj.ChangeOrderGroup.BusinessTripOrders.FirstOrDefault();
      DirRX.ExpenseReports.PublicFunctions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), order, e.Action.LocalizedName, e, false);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}