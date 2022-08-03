using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripSignOrderAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripSignOrderAssignmentActions
  {
    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(BusinessTripSignOrderAssignments.Resources.ReWorkNoActiveTextErrorText);
        return;
      }
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void CancelTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = BusinessTripApprovalTasks.As(_obj.Task);
      var cancelDone = Functions.BusinessTripApprovalTask.CancelBusinessTrip(task, e, task.OrderSignerPerformer);
      if (cancelDone)
      {
        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTripApprovalTasks.Resources.TripCancelSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
    }

    public virtual bool CanCancelTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Кнопка доступна только для задания в работе
      return _obj.Status == Status.InProcess;
    }

    public virtual void ForceReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForceReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }


    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var order = _obj.OrderGroup.BusinessTripOrders.FirstOrDefault();
      // Неподписанным всегда будет только 1 приказ из всех приказов об изменении. Поиск такого приказа и подписание.
      var orders = _obj.OtherGroup.All.Where(d => DirRX.BusinessTrips.BusinessTripOrders.Is(d)).Cast<DirRX.BusinessTrips.IBusinessTripOrder>();
      if (orders.Any())
        order = orders.Where(o => o.InternalApprovalState != DirRX.BusinessTrips.BusinessTripOrder.InternalApprovalState.Signed).FirstOrDefault();
      
      DirRX.ExpenseReports.PublicFunctions.Module.ApproveDocument(Sungero.Workflow.Assignments.As(_obj), order, e.Action.LocalizedName, e, false);
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}