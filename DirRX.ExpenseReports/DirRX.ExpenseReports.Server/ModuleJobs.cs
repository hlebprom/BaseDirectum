using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.ExpenseReports.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Авансовые отчеты. Загрузка валют.
    /// </summary>
    public virtual void DownloadRates()
    {
      Functions.Module.DownloadRatesFromSite();
    }

  }
}