using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.CurrencyRate;

namespace DirRX.ExpenseReports.Shared
{
  partial class CurrencyRateFunctions
  {
    /// <summary>
    /// Заполнить наименование записи для курса.
    /// </summary>
    public void FillName()
    {
      // Пример наименования: "Курс Евро на 18.03.2022"
      var currencyName = string.Empty;
      if (_obj.Currency != null)
        currencyName = _obj.Currency.DisplayValue;
      
      var dateValue = string.Empty;
      if (_obj.Date.HasValue)
        dateValue = _obj.Date.Value.ToShortDateString();
      
      var name = string.Empty;
      
      if (string.IsNullOrEmpty(currencyName) && string.IsNullOrEmpty(dateValue))
        name = CurrencyRates.Resources.NameAutoText;
      else
      {
        using (TenantInfo.Culture.SwitchTo())
        {
          name = CurrencyRates.Resources.NameFormat(currencyName, dateValue);
        }
      }
      _obj.Name = name;
    }
  }
}