using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripPrepareReportAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripPrepareReportAssignmentFunctions
  {

    /// <summary>
    /// Подтянуть данные таблицы расходов из авансового отчета.
    /// </summary>
    public void GetExpensesFromReport()
    {
      Functions.BusinessTripPrepareReportAssignment.Remote.GetExpensesFromReport(_obj);
    }

    /// <summary>
    /// Добавить суточные в расходы.
    /// </summary>
    public void AddPerDiem()
    {
      Functions.BusinessTripPrepareReportAssignment.Remote.AddPerDiem(_obj);
    }

  }
}