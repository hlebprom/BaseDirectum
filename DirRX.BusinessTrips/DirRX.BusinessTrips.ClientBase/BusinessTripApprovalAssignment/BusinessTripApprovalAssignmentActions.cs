using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripApprovalAssignmentActions
  {

    public virtual void CancelTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var task = BusinessTripApprovalTasks.As(_obj.Task);
      var cancelDone = Functions.BusinessTripApprovalTask.CancelBusinessTrip(task, e, task.BossApprovalPerformer);
      if (cancelDone)
      {
        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTripApprovalTasks.Resources.TripCancelSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
    }

    public virtual bool CanCancelTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Кнопка доступна только для задания руководителя в работе
      return _obj.Status == Status.InProcess && _obj.NeedShowCancelTripButton.Value;
    }


    public virtual void ForceReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForceReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ReWork(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ActiveText))
      {
        e.AddError(BusinessTripApprovalAssignments.Resources.ReWorkNoActiveTextErrorText);
        return;
      }
    }

    public virtual bool CanReWork(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}