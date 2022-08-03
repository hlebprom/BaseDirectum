using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTrip;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Скрыть факт даты, если Состояние командировки отлично от Завершена
      if (_obj.BusinessTripStatus != BusinessTripStatus.Finished)
      {
        _obj.State.Properties.FactDepartureDate.IsVisible = false;
        _obj.State.Properties.FactReturnDate.IsVisible = false;
      }
      
      var isAnyLimitExceeded = false;
      
      foreach(var expenseRow in _obj.Expenses)
      {
        // Подсветить превышение лимитов
        Functions.BusinessTrip.CheckLimit(_obj, expenseRow);
        
        if (expenseRow.Limit.HasValue && expenseRow.ExpenseSum.Value > expenseRow.Limit.Value)
          isAnyLimitExceeded = true;
      }
      
      if (isAnyLimitExceeded)
        e.AddInformation(DirRX.ExpenseReports.ExpenseReports.Resources.LimitExceededErrorText);
    }
  }


}