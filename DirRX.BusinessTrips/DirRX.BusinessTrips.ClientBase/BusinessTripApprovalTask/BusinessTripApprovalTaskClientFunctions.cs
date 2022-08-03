using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalTask;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripApprovalTaskFunctions
  {

    /// <summary>
    /// Добавить суточные в расходы.
    /// </summary>
    public void AddPerDiem()
    {
      Functions.BusinessTripApprovalTask.Remote.AddPerDiem(_obj);
    }
    
    /// <summary>
    /// Запустить изменение командировки с запросом причины.
    /// </summary>
    /// <param name="e">Аргументы действия.</param>
    /// <returns>True - в случае успешного запуска изменения, иначе False.</returns>
    [Public]
    public virtual bool ChangeBusinessTrip(Sungero.Core.IValidationArgs e)
    {
      var businessTrip = Functions.BusinessTripApprovalTask.Remote.GetBusinessTrip(_obj);
      if (businessTrip.BusinessTripStatus == DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Finished
          || businessTrip.BusinessTripStatus == DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Canceled)
      {
        e.AddWarning(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripChangelTooLate);
        return false;
      }
      
      if (Sungero.Company.Employees.Current == null || !Equals(_obj.Employee, Sungero.Company.Employees.Current))
      {
        e.AddWarning(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripChangeNoProcessOwner);
        return false;
      }
      
      // Если инициировано изменение (уже есть задание на доработку), не давать запускать его снова
      if (_obj.NeedChange == true)
      {
        e.AddWarning(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripChangeAlreadyStarted);
        return false;
      }
      
      // Запросить причину изменения
      var changeReasonDialog = Dialogs.CreateInputDialog(BusinessTripApprovalTasks.Resources.ChangeConfirmation);
      var changeReason = changeReasonDialog.AddMultilineString(BusinessTripApprovalTasks.Resources.Reason, true);
      changeReasonDialog.SetOnButtonClick(args =>
                                          {
                                            if (string.IsNullOrWhiteSpace(changeReason.Value))
                                              args.AddError(BusinessTripApprovalTasks.Resources.EmptyChangeReason, changeReason);
                                          });
      
      if (changeReasonDialog.Show() == DialogButtons.Ok)
      {
        Functions.BusinessTripApprovalTask.Remote.ChangeBusinessTrip(_obj, changeReason.Value);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Запустить отмену командировки с запросом причины.
    /// </summary>
    /// <param name="e">Аргументы действия.</param>
    /// <param name="cancelInitiator">Инициатор отмены.</param>
    /// <returns>True - в случае успешного запуска отмены, False - в случае отмены действия пользователем.</returns>
    [Public]
    public virtual bool CancelBusinessTrip(Sungero.Core.IValidationArgs e, IRecipient cancelInitiator)
    {
      // Отменить может только руководитель, сам сотрудник или их заместители
      var manager = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetManager(_obj.Employee);
      if (manager == null)
      {
        e.AddError(DirRX.BusinessTrips.Resources.NoManagerInDepartmentErrorFormat(_obj.Employee.Department.Name));
        return false;
      }
      
      var currentEmployee = Sungero.Company.Employees.Current;
      if (currentEmployee == null
          || (!Equals(_obj.Employee, currentEmployee)
              && !Sungero.CoreEntities.Substitutions.ActiveUsersWhoSubstitute(currentEmployee).Where(u => Equals(u, Users.Current)).Any()
              && !Equals(Users.Current, manager)
              && !Sungero.CoreEntities.Substitutions.ActiveUsersWhoSubstitute(manager).Where(u => Equals(u, Users.Current)).Any()
             )
         )
      {
        e.AddWarning(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripCancelEmployeeOrManagerCanDo);
        return false;
      }
      
      // Проверить, что отмена ещё не выполнялась
      if (Functions.Module.Remote.GetCancelTasksByBusinessTripApprovalTask(_obj).Any())
      {
        e.AddWarning(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripCancelAlredyInitialized);
        return false;
      }
      
      // Отмена доступена до завершения командировки
      var businessTrip = Functions.BusinessTripApprovalTask.Remote.GetBusinessTrip(_obj);
      if (businessTrip.BusinessTripStatus == DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Finished
          || businessTrip.BusinessTripStatus == DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Canceled)
      {
        e.AddWarning(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripCancelTooLate);
        return false;
      }
      
      // Запросить причину отмены
      var cancelReasonDialog = Dialogs.CreateInputDialog(BusinessTripApprovalTasks.Resources.CancelConfirmation);
      var cancelReason = cancelReasonDialog.AddMultilineString(BusinessTripApprovalTasks.Resources.Reason, true);
      cancelReasonDialog.SetOnButtonClick(args =>
                                          {
                                            if (string.IsNullOrWhiteSpace(cancelReason.Value))
                                              args.AddError(BusinessTripApprovalTasks.Resources.EmptyCancelReason, cancelReason);
                                          });
      
      if (cancelReasonDialog.Show() == DialogButtons.Ok)
      {
        Functions.BusinessTripApprovalTask.Remote.CreateBusinessTripCancelTask(_obj, cancelReason.Value, cancelInitiator);
        return true;
      }
      return false;
    }
  }
}