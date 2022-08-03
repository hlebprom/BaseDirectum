using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripReWorkAssignment;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripReWorkAssignmentFunctions
  {

    /// <summary>
    /// Добавить суточные в расходы.
    /// </summary>
    public void AddPerDiem()
    {
      Functions.BusinessTripReWorkAssignment.Remote.AddPerDiem(_obj);
    }

  }
}