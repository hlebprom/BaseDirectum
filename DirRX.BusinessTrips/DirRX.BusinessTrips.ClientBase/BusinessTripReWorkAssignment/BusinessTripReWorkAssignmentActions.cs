using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripReWorkAssignment;

namespace DirRX.BusinessTrips.Client
{
  internal static class BusinessTripReWorkAssignmentExpensesStaticActions
  {

    public static bool CanAddPerDiem(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public static void AddPerDiem(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.Module.CalculateExpensesInReWorkAssignment(BusinessTripReWorkAssignments.As(e.RootEntity));
    }
  }

  partial class BusinessTripReWorkAssignmentActions
  {


    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}