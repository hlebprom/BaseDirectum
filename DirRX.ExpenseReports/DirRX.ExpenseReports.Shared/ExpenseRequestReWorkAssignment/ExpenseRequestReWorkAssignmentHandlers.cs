using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseRequestReWorkAssignment;

namespace DirRX.ExpenseReports
{
  partial class ExpenseRequestReWorkAssignmentExpensesSharedCollectionHandlers
  {

    public virtual void ExpensesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.ExpenseRequestReWorkAssignment.ReCountSum(_obj);
    }
  }

  partial class ExpenseRequestReWorkAssignmentExpensesSharedHandlers
  {

    public virtual void ExpensesExpenseTypeChanged(DirRX.ExpenseReports.Shared.ExpenseRequestReWorkAssignmentExpensesExpenseTypeChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.LimitSetting = DirRX.ExpenseReports.Functions.Module.Remote.GetLimitSetting(e.NewValue,
                                                                                         DirRX.ExpenseReports.ExpenseRequestApprovalTasks.As(_obj.ExpenseRequestReWorkAssignment.Task).Employee,
                                                                                         Sungero.Commons.Cities.Null);
    }

    public virtual void ExpensesExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ExpenseRequestReWorkAssignment.ReCountSum(_obj.ExpenseRequestReWorkAssignment);
      Functions.ExpenseRequestReWorkAssignment.CheckLimit(_obj.ExpenseRequestReWorkAssignment, _obj);
    }
  }

  partial class ExpenseRequestReWorkAssignmentSharedHandlers
  {

  }
}