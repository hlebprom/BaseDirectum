using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalAssignment;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripApprovalAssignmentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.NeedShowCancelTripButton = false;
    }
  }

}