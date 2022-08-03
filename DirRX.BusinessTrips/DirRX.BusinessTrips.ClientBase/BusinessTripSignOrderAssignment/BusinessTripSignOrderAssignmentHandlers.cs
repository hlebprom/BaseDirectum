using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripSignOrderAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripSignOrderAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.ExpenseReportGroup.IsVisible = _obj.ExpenseReportGroup.All.Any();
    }
  }

}