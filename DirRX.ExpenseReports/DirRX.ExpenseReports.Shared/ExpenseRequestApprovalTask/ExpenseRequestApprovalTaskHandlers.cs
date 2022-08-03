using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestApprovalTask;

namespace DirRX.ExpenseReports
{
  partial class ExpenseRequestApprovalTaskExpensesSharedCollectionHandlers
  {

    public virtual void ExpensesAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      Functions.ExpenseRequestApprovalTask.ReCountSum(_obj);
    }

    public virtual void ExpensesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.ExpenseRequestApprovalTask.ReCountSum(_obj);
    }
  }

  partial class ExpenseRequestApprovalTaskExpensesSharedHandlers
  {

    public virtual void ExpensesExpenseTypeChanged(DirRX.ExpenseReports.Shared.ExpenseRequestApprovalTaskExpensesExpenseTypeChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.LimitSetting = DirRX.ExpenseReports.Functions.Module.Remote.GetLimitSetting(e.NewValue, _obj.ExpenseRequestApprovalTask.Employee, Sungero.Commons.Cities.Null);
    }

    public virtual void ExpensesExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ExpenseRequestApprovalTask.ReCountSum(_obj.ExpenseRequestApprovalTask);
      Functions.ExpenseRequestApprovalTask.CheckLimit(_obj.ExpenseRequestApprovalTask, _obj);
    }
  }

  partial class ExpenseRequestApprovalTaskSharedHandlers
  {

    public virtual void ExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ExpenseRequestApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

    public virtual void PurposeChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.ExpenseRequestApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

    public virtual void EmployeeChanged(DirRX.ExpenseReports.Shared.ExpenseRequestApprovalTaskEmployeeChangedEventArgs e)
    {      
      Functions.ExpenseRequestApprovalTask.Remote.UpdateTaskSubject(_obj);
    }

  }
}