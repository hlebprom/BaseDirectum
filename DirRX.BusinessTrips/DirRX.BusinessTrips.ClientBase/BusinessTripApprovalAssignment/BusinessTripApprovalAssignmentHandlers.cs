using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripApprovalAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Attachments.OrderGroup.IsVisible = _obj.OrderGroup.All.Any();
      _obj.State.Attachments.ExpenseReportGroup.IsVisible = _obj.ExpenseReportGroup.All.Any();
    }
  }

}