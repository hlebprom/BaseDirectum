using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.CurrencyRate;

namespace DirRX.ExpenseReports
{
  partial class CurrencyRateClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      /// Закрыть доступ к контролам, если курс создан автоматически.
      _obj.State.Properties.Currency.IsEnabled = !_obj.IsAuto.Value;
      _obj.State.Properties.Date.IsEnabled = !_obj.IsAuto.Value;
      _obj.State.Properties.Rate.IsEnabled = !_obj.IsAuto.Value;
    }

    public virtual void RateValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value <= 0)
        e.AddError(CurrencyRates.Resources.RateMustBePositive);
    }

  }
}