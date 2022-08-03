using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.ExpenseReports.Shared
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Пересчитать сумму в рубли.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="total">Сумма в валюте.</param>
    /// <param name="date">Дата.</param>
    /// <returns>Сумма в рублях.</returns>
    [Public]
    public virtual double? ConvertIntoRUB(Sungero.Commons.ICurrency currency, double? total, DateTime? date)
    {
      if (currency != null && total.HasValue && date.HasValue)
      {
        var currencyRUB = Functions.Module.Remote.GetCurrencyRUB();
        
        if (!Equals(currencyRUB, currency))
        {
          var rate = Functions.Module.Remote.GetCurrencyRateForDate(currency, date.Value);
          if (rate.HasValue)
            return Math.Round((total.Value * rate.Value), 2);
        }
        else
          return total;
      }
      
      return null;
    }
  }
}