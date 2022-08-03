using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalTask;
using Sungero.Workflow;

namespace DirRX.BusinessTrips.Client
{
  internal static class BusinessTripApprovalTaskExpensesStaticActions
  {
    public static void AddPerDiem(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.Module.CalculateExpensesInTask(DirRX.BusinessTrips.BusinessTripApprovalTasks.As(e.RootEntity));
    }

    public static bool CanAddPerDiem(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }
  }


  partial class BusinessTripApprovalTaskActions
  {


    public virtual void CancelTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var cancelDone = Functions.BusinessTripApprovalTask.CancelBusinessTrip(_obj, e, Sungero.Company.Employees.Current);
      if (cancelDone)
      {
        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTripApprovalTasks.Resources.TripCancelSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
    }

    public virtual bool CanCancelTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Кнопка доступна только для задачи в работе
      return _obj.Status == Status.InProcess;
    }

    public virtual void ChangeTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var cahangeDone = Functions.BusinessTripApprovalTask.ChangeBusinessTrip(_obj, e);
      if (cahangeDone)
      {
        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTripApprovalTasks.Resources.TripChangeSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
    }

    public virtual bool CanChangeTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Изменение командировки доступно, если задача на согласование командировки в работе
      return _obj.Status == Status.InProcess;
    }

    public override void Restart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Restart(e);
    }

    public override bool CanRestart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Рестарт доступен только администратору
      return Users.Current.IncludedIn(Roles.Administrators);
    }


    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Abort(e);
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Прекращение доступно только администратору
      return Users.Current.IncludedIn(Roles.Administrators);
    }

  }
}