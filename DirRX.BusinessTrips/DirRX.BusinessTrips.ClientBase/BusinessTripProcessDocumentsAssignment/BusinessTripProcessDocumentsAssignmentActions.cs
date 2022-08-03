using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripProcessDocumentsAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripProcessDocumentsAssignmentActions
  {
    public virtual void ForceReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForceReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // Подписать приказы
      var orders = new List<DirRX.BusinessTrips.IBusinessTripOrder> ();
      orders.Add(_obj.OrderGroup.BusinessTripOrders.SingleOrDefault());
      orders.AddRange(_obj.OtherGroup.All.Where(d => DirRX.BusinessTrips.BusinessTripOrders.Is(d)).Cast<DirRX.BusinessTrips.IBusinessTripOrder>());
      foreach (var order in orders)
      {
        DirRX.ExpenseReports.PublicFunctions.Module.ApproveDocument(_obj,
                                                                    order,
                                                                    BusinessTripProcessDocumentsAssignments.Resources.Acquainted,
                                                                    e,
                                                                    true);
      }
      
      // Подписать служебную записку при наличии
      var businessTripMemo = BusinessTripApprovalTasks.As(_obj.Task).BusinessTripMemo;
      if (businessTripMemo != null)
        DirRX.ExpenseReports.PublicFunctions.Module.ApproveDocument(_obj,
                                                                    businessTripMemo,
                                                                    String.Empty,
                                                                    e,
                                                                    true);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}