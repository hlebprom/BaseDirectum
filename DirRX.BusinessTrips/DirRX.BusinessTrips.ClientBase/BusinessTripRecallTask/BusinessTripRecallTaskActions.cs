using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripRecallTask;

namespace DirRX.BusinessTrips.Client
{
  partial class BusinessTripRecallTaskActions
  {
    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Abort(e);
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Прекращение доступно только администратору
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public override void Restart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Restart(e);
    }

    public override bool CanRestart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Рестарт доступен только администратору
      return Users.Current.IncludedIn(Roles.Administrators);
    }

  }

}