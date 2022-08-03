using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.CurrencyRate;

namespace DirRX.ExpenseReports
{
  partial class CurrencyRateServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.IsAuto = false;
    	_obj.Date = Calendar.Today;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var rate = CurrencyRates.GetAll(cr => Equals(cr.Currency, _obj.Currency) && cr.Date == _obj.Date.Value && !Equals(cr, _obj)).FirstOrDefault();
    	if (rate != null)
    		e.AddError(CurrencyRates.Resources.AnotherRateOnDateFoundFormat(_obj.Currency, _obj.Date.Value.ToShortDateString()));
    }
  }

}