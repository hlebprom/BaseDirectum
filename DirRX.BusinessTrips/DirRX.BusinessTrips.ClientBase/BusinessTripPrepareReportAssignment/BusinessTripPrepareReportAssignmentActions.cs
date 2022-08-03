using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripPrepareReportAssignment;

namespace DirRX.BusinessTrips.Client
{
  internal static class BusinessTripPrepareReportAssignmentExpensesStaticActions
  {
    public static void GetExpensesFromReport(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.BusinessTripPrepareReportAssignment.GetExpensesFromReport(BusinessTripPrepareReportAssignments.As(e.RootEntity));
    }

    public static bool CanGetExpensesFromReport(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public static bool CanAddPerDiem(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public static void AddPerDiem(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.Module.CalculateExpensesInPrepareReportAssignment(BusinessTripPrepareReportAssignments.As(e.RootEntity));
    }
  }

  partial class BusinessTripPrepareReportAssignmentAnyChildEntityCollectionActions
  {
    public override void DeleteChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.DeleteChildEntity(e);
    }

    public override bool CanDeleteChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      // Заблокировать удаление строк из коллекции Route (маршрут командировки)
      var root = BusinessTripPrepareReportAssignments.As(e.RootEntity);
      return (root != null && _all == root.Route)
        ? false
        : base.CanDeleteChildEntity(e);;
    }

  }

  partial class BusinessTripPrepareReportAssignmentAnyChildEntityActions
  {
    public override void CopyChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.CopyChildEntity(e);
    }

    public override bool CanCopyChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      // Заблокировать копирование строк коллекции Route (маршрут командировки)
      var root = BusinessTripPrepareReportAssignments.As(e.RootEntity);
      return (root != null && _all == root.Route)
        ? false
        : base.CanCopyChildEntity(e);
    }

    public override void AddChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.AddChildEntity(e);
    }

    public override bool CanAddChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      // Заблокировать добавление строк в коллекцию Route (маршрут командировки)
      var root = BusinessTripPrepareReportAssignments.As(e.RootEntity);
      return (root != null && _all == root.Route)
        ? false
        : base.CanAddChildEntity(e);
    }

  }

  partial class BusinessTripPrepareReportAssignmentActions
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
      // Если факт даты отличаются от исходных, запросить причину изменения для приказа об изменении командировки
      var task = BusinessTripApprovalTasks.As(_obj.Task);
      if (task.DepartureDate != _obj.FactDepartureDate || task.ReturnDate != _obj.FactReturnDate)
      {
        if (string.IsNullOrEmpty(_obj.ActiveText))
        {
          e.AddError(BusinessTripPrepareReportAssignments.Resources.EmptyChangeFactDatesReason);
          return;
        }
        task.ChangeReason = _obj.ActiveText;
      }
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}