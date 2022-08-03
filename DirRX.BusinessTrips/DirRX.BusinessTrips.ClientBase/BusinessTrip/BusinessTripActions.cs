using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTrip;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripActions
  {
    public virtual void GetTasks(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.BusinessTrip.Remote.GetBusinessTripTasks(_obj).Show();
    }

    public virtual bool CanGetTasks(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void RecallTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      // Отозвать может только руководитель или его заместители
      var manager = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetManager(_obj.Employee);
      if (manager == null)
      {
        e.AddError(DirRX.BusinessTrips.Resources.NoManagerInDepartmentErrorFormat(_obj.Employee.Department.Name));
        return;
      }
      
      var currentEmployee = Sungero.Company.Employees.Current;
      if (currentEmployee == null
          || (!Equals(currentEmployee, manager)
              && !Sungero.CoreEntities.Substitutions.ActiveUsersWhoSubstitute(manager).Where(u => Equals(u, Users.Current)).Any()
             )
         )
      {
        e.AddWarning(BusinessTrips.Resources.TripRecallManagerCanDo);
        return;
      }
      
      // Проверить, что отзыв ещё не выполнялся
      if (Functions.Module.Remote.GetRecallTasksByBusinessTrip(_obj).Any())
      {
        e.AddWarning(BusinessTrips.Resources.TripRecallAlredyInitialized);
        return;
      }
      
      // Отзыв доступен только после начала командировки
      if (Calendar.Today < _obj.DepartureDate || Calendar.Today > _obj.ReturnDate)
      {
        e.AddWarning(BusinessTrips.Resources.TripRecallWaitForBegin);
        return;
      }
      
      // Запросить причину отзыва из командировки
      var recallReasonDialog = Dialogs.CreateInputDialog(BusinessTrips.Resources.RecallConfirmation);
      var recallReason = recallReasonDialog.AddMultilineString(BusinessTrips.Resources.RecallReason, true);
      var recallDate = recallReasonDialog.AddDate(BusinessTrips.Resources.RecallDate, true);
      recallReasonDialog.SetOnButtonClick(args =>
                                          {
                                            if (string.IsNullOrWhiteSpace(recallReason.Value))
                                              args.AddError(BusinessTrips.Resources.EmptyRecallReason, recallReason);
                                            
                                            if (recallDate.Value < Calendar.Today)
                                              args.AddError(BusinessTrips.Resources.RecallPastDateError, recallDate);
                                            
                                            if (recallDate.Value >= _obj.ReturnDate)
                                              args.AddError(BusinessTrips.Resources.RecallFeatureDateError, recallDate);
                                            
                                          });
      
      if (recallReasonDialog.Show() == DialogButtons.Ok)
      {
        var tripRecallTask = BusinessTripRecallTasks.Create();
        tripRecallTask.RecallReason = recallReason.Value;
        tripRecallTask.RecallDate = recallDate.Value;
        tripRecallTask.RecallInitiator = Users.Current;
        tripRecallTask.BusinessTrip = _obj;
        tripRecallTask.Start();

        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTrips.Resources.TripRecallSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
    }

    public virtual bool CanRecallTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Кнопка доступна до завершения командировки
      return _obj.BusinessTripStatus != BusinessTripStatus.Finished && _obj.BusinessTripStatus != BusinessTripStatus.Canceled;
    }

    public virtual void ChangeTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var cahangeDone = Functions.BusinessTripApprovalTask.ChangeBusinessTrip(Functions.BusinessTrip.Remote.GetBusinessTripApprovalTask(_obj), e);
      if (cahangeDone)
      {
        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTripApprovalTasks.Resources.TripChangeSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
    }

    public virtual bool CanChangeTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Кнопка доступна до завершения командировки
      return _obj.BusinessTripStatus != BusinessTripStatus.Finished && _obj.BusinessTripStatus != BusinessTripStatus.Canceled;
    }

    public virtual void CancelTrip(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var cancelDone = Functions.BusinessTripApprovalTask.CancelBusinessTrip(Functions.BusinessTrip.Remote.GetBusinessTripApprovalTask(_obj), 
                                                                             e, 
                                                                             Sungero.Company.Employees.Current);
      if (cancelDone)
      {
        // ShowMessage и закрытие формы нужны чтобы убрать блокировку и пользователь всё-таки увидел результат работы кнопки.
        Dialogs.ShowMessage(BusinessTripApprovalTasks.Resources.TripCancelSuccessfully, MessageType.Information);
        e.CloseFormAfterAction = true;
      }
      
    }

    public virtual bool CanCancelTrip(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Кнопка доступна до завершения командировки
      return _obj.BusinessTripStatus != BusinessTripStatus.Finished && _obj.BusinessTripStatus != BusinessTripStatus.Canceled;
    }

    public virtual void GetDocuments(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.BusinessTrip.Remote.GetBusinessTripDocuments(_obj).Show();
    }

    public virtual bool CanGetDocuments(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }


}