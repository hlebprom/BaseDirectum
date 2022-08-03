using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripPrepareReportAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripPrepareReportAssignmentExpensesSharedCollectionHandlers
  {

    public virtual void ExpensesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.BusinessTripPrepareReportAssignment.ReCountSums(_obj);
      Functions.BusinessTripPrepareReportAssignment.ReCountDocsAndPages(_obj);
    }
  }

  partial class BusinessTripPrepareReportAssignmentExpensesSharedHandlers
  {

    public virtual void ExpensesExpenseTypeChanged(DirRX.BusinessTrips.Shared.BusinessTripPrepareReportAssignmentExpensesExpenseTypeChangedEventArgs e)
    {
      if (e.NewValue != null
          && !Equals(e.NewValue, DirRX.BusinessTrips.Functions.Module.Remote.GetPerDiemExpenseType())
          && !Equals(e.NewValue, DirRX.BusinessTrips.Functions.Module.Remote.GetHotelExpenseType()))
      {
        var assignment = BusinessTripPrepareReportAssignments.As(_obj.RootEntity);        
        _obj.LimitSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(e.NewValue,
                                                                                              DirRX.BusinessTrips.BusinessTripApprovalTasks.As(assignment.Task).Employee,
                                                                                              Sungero.Commons.Cities.Null);
      }
    }
    
    public virtual void ExpensesLimitSettingChanged(DirRX.BusinessTrips.Shared.BusinessTripPrepareReportAssignmentExpensesLimitSettingChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.Limit = e.NewValue.Limit;
    }
    
    public virtual void ExpensesExpenseSumInCurrencyChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      _obj.ExpenseSum = DirRX.ExpenseReports.PublicFunctions.Module.ConvertIntoRUB(_obj.Currency, _obj.ExpenseSumInCurrency, _obj.ExpenseDate);
    }

    public virtual void ExpensesExpenseDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      _obj.ExpenseSum = DirRX.ExpenseReports.PublicFunctions.Module.ConvertIntoRUB(_obj.Currency, _obj.ExpenseSumInCurrency, _obj.ExpenseDate);
    }

    public virtual void ExpensesCurrencyChanged(DirRX.BusinessTrips.Shared.BusinessTripPrepareReportAssignmentExpensesCurrencyChangedEventArgs e)
    {
      _obj.ExpenseSum = DirRX.ExpenseReports.PublicFunctions.Module.ConvertIntoRUB(_obj.Currency, _obj.ExpenseSumInCurrency, _obj.ExpenseDate);
    }

    public virtual void ExpensesSupportingDocChanged(DirRX.BusinessTrips.Shared.BusinessTripPrepareReportAssignmentExpensesSupportingDocChangedEventArgs e)
    {
      Functions.BusinessTripPrepareReportAssignment.ReCountDocsAndPages(_obj.BusinessTripPrepareReportAssignment);
      
      if (e.NewValue != null)
      {
        if (!_obj.ExpenseDate.HasValue)
          _obj.ExpenseDate = e.NewValue.Date;
        
        if (string.IsNullOrEmpty(_obj.ExpenseDescription))
          _obj.ExpenseDescription = e.NewValue.Subject;
        
        if (!_obj.ExpenseSumInCurrency.HasValue)
          _obj.ExpenseSumInCurrency = e.NewValue.Sum;
        
        if (_obj.Currency == null)
          _obj.Currency = e.NewValue.Currency;
      }
    }

    public virtual void ExpensesExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.BusinessTripPrepareReportAssignment.ReCountSums(_obj.BusinessTripPrepareReportAssignment);
      Functions.BusinessTripPrepareReportAssignment.CheckLimit(_obj.BusinessTripPrepareReportAssignment, _obj);
    }
  }


  partial class BusinessTripPrepareReportAssignmentSharedHandlers
  {

    public virtual void GettedMoneyChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.BusinessTripPrepareReportAssignment.ReCountSums(_obj);
    }

  }
}