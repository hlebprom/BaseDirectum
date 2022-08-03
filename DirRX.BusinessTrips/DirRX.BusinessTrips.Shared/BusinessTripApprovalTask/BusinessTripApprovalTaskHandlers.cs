using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalTask;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripApprovalTaskExpensesSharedHandlers
  {

    public virtual void ExpensesExpenseTypeChanged(DirRX.BusinessTrips.Shared.BusinessTripApprovalTaskExpensesExpenseTypeChangedEventArgs e)
    {
      if (e.NewValue != null
          && !Equals(e.NewValue, DirRX.BusinessTrips.Functions.Module.Remote.GetPerDiemExpenseType())
          && !Equals(e.NewValue, DirRX.BusinessTrips.Functions.Module.Remote.GetHotelExpenseType()))
        _obj.LimitSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(e.NewValue, BusinessTripApprovalTasks.As(_obj.RootEntity).Employee, Sungero.Commons.Cities.Null);
    }

    public virtual void ExpensesLimitSettingChanged(DirRX.BusinessTrips.Shared.BusinessTripApprovalTaskExpensesLimitSettingChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.Limit = e.NewValue.Limit;
    }

    public virtual void ExpensesExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.ReCountSum(_obj.BusinessTripApprovalTask);

      Functions.BusinessTripApprovalTask.CheckLimit(_obj.BusinessTripApprovalTask, _obj);
    }
  }

  partial class BusinessTripApprovalTaskExpensesSharedCollectionHandlers
  {

    public virtual void ExpensesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.ReCountSum(_obj);
    }

    public virtual void ExpensesAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.ReCountSum(_obj);
    }
  }

  partial class BusinessTripApprovalTaskRouteSharedHandlers
  {

    public virtual void RouteOrgChanged(DirRX.BusinessTrips.Shared.BusinessTripApprovalTaskRouteOrgChangedEventArgs e)
    {
      // Заполнить Пункт назначения от выбранной организации
      if (e.NewValue != null)
      {
        _obj.Destination = e.NewValue.City;
      }
    }
  }

  partial class BusinessTripApprovalTaskSharedHandlers
  {

    public virtual void PurposeChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

    public virtual void ReturnDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

    public virtual void DepartureDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

    public virtual void EmployeeChanged(DirRX.BusinessTrips.Shared.BusinessTripApprovalTaskEmployeeChangedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

    public virtual void BusinessTripMemoChanged(DirRX.BusinessTrips.Shared.BusinessTripApprovalTaskBusinessTripMemoChangedEventArgs e)
    {
      // Очищение группы вложений в случае удаления СЗ, чтобы в блоке задачи не выдавались права на помеченный на удаление объект.
      if (e.OldValue != null)
        _obj.OtherGroup.All.Remove(e.OldValue);
      
      if (e.NewValue != null)
        _obj.OtherGroup.All.Add(e.NewValue);
    }
    
    public virtual void ByCarChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      Functions.BusinessTripApprovalTask.ProcessCarProperties(_obj);
    }

  }
}