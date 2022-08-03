using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTrip;

namespace DirRX.BusinessTrips
{


  partial class BusinessTripSharedHandlers
  {

    public virtual void FinanceSourceChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      // HACK Заполнить источник финансирования пробелом, чтобы в шаблон корректно подставлялось пустое значение
      if (String.IsNullOrEmpty(e.NewValue))
        _obj.FinanceSource = " ";
    }

    public virtual void ReturnDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      Functions.BusinessTrip.UpdateName(_obj);
    }

    public virtual void DepartureDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      Functions.BusinessTrip.UpdateName(_obj);
    }

    public virtual void PurposeChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.BusinessTrip.UpdateName(_obj);
    }

    public virtual void EmployeeChanged(DirRX.BusinessTrips.Shared.BusinessTripEmployeeChangedEventArgs e)
    {
      Functions.BusinessTrip.UpdateName(_obj);
      if (e.NewValue != null && e.NewValue != e.OldValue)
      {
        _obj.Department = e.NewValue.Department;
        _obj.BusinessUnit = _obj.Department.BusinessUnit;
      }
    }

  }
}