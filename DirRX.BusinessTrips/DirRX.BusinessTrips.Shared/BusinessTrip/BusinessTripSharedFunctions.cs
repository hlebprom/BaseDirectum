using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTrip;

namespace DirRX.BusinessTrips.Shared
{
  partial class BusinessTripFunctions
  {
    /// <summary>
    /// Проверить и подсветить превышение лимита по строке расхода.
    /// </summary>
    /// <param name="expenseRow">Строка таблицы расходов.</param>
    public void CheckLimit(IBusinessTripExpenses expenseRow)
    {
      if (expenseRow.ExpenseSum.HasValue && expenseRow.Limit.HasValue
          && expenseRow.ExpenseSum.Value > expenseRow.Limit.Value)
      {
        expenseRow.State.Properties.ExpenseSum.HighlightColor = Sungero.Core.Colors.Parse(DirRX.ExpenseReports.PublicConstants.Module.LimitHighlightColorRed);
        expenseRow.State.Properties.LimitSetting.HighlightColor = Sungero.Core.Colors.Parse(DirRX.ExpenseReports.PublicConstants.Module.LimitHighlightColorRed);
      }
      else
      {
        expenseRow.State.Properties.ExpenseSum.HighlightColor = Colors.Empty;
        expenseRow.State.Properties.LimitSetting.HighlightColor = Colors.Empty;
      }
    }
    
    /// <summary>
    /// Изменение имени.
    /// </summary>
    public virtual void UpdateName()
    {
      _obj.Name = BusinessTrips.Resources.NameTemplateFormat(Functions.Module.Remote.FormatBusinessTripName(_obj.Employee, _obj.DepartureDate, _obj.ReturnDate, _obj.Purpose));
    }

  }
}