using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportApprovalTask;

namespace DirRX.ExpenseReports.Client
{
  partial class ExpenseReportApprovalTaskActions
  {
    public override void Restart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Restart(e);
    }

    public override bool CanRestart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Рестарт доступен только администратору
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Abort(e);
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Прекращение доступно только администратору
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public override void Start(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      // Если пользователь отправляет на согласование авансовый отчет вручную, то задание на подготовку не требуется.
      _obj.NeedPrepare = false;
      
      base.Start(e);
    }

    public override bool CanStart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanStart(e);
    }

  }

}