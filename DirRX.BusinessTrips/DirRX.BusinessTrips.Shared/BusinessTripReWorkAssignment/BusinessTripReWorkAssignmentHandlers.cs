using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripReWorkAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripReWorkAssignmentExpensesSharedCollectionHandlers
  {

    public virtual void ExpensesAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      Functions.BusinessTripReWorkAssignment.ReCountSum(_obj);
    }

    public virtual void ExpensesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.BusinessTripReWorkAssignment.ReCountSum(_obj);
    }
  }

  partial class BusinessTripReWorkAssignmentExpensesSharedHandlers
  {

    public virtual void ExpensesExpenseTypeChanged(DirRX.BusinessTrips.Shared.BusinessTripReWorkAssignmentExpensesExpenseTypeChangedEventArgs e)
    {
      if (e.NewValue != null
          && !Equals(e.NewValue, DirRX.BusinessTrips.Functions.Module.Remote.GetPerDiemExpenseType())
          && !Equals(e.NewValue, DirRX.BusinessTrips.Functions.Module.Remote.GetHotelExpenseType()))
      {
        var assignment = BusinessTripReWorkAssignments.As(_obj.RootEntity);
        _obj.LimitSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(e.NewValue, DirRX.BusinessTrips.BusinessTripApprovalTasks.As(assignment.Task).Employee, Sungero.Commons.Cities.Null);
      }
    }

    public virtual void ExpensesLimitSettingChanged(DirRX.BusinessTrips.Shared.BusinessTripReWorkAssignmentExpensesLimitSettingChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.Limit = e.NewValue.Limit;
    }

    public virtual void ExpensesExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.BusinessTripReWorkAssignment.ReCountSum(_obj.BusinessTripReWorkAssignment);

      Functions.BusinessTripReWorkAssignment.CheckLimit(_obj.BusinessTripReWorkAssignment, _obj);
    }
  }

  partial class BusinessTripReWorkAssignmentRouteSharedHandlers
  {

    public virtual void RouteOrgChanged(DirRX.BusinessTrips.Shared.BusinessTripReWorkAssignmentRouteOrgChangedEventArgs e)
    {
      // Заполнить Пункт назначения от выбранной организации
      if (e.NewValue != null)
      {
        _obj.Destination = e.NewValue.City;
      }
    }
  }

  partial class BusinessTripReWorkAssignmentSharedHandlers
  {

    public virtual void ByCarChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      Functions.BusinessTripReWorkAssignment.ProcessCarProperties(_obj);
    }

  }
}