using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.CurrencyRate;

namespace DirRX.ExpenseReports
{
  partial class CurrencyRateSharedHandlers
  {

    public virtual void DateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      Functions.CurrencyRate.FillName(_obj);
    }

    public virtual void CurrencyChanged(DirRX.ExpenseReports.Shared.CurrencyRateCurrencyChangedEventArgs e)
    {
      Functions.CurrencyRate.FillName(_obj);
    }

  }
}