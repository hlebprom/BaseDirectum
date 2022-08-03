using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReport;
using System.Globalization;

namespace DirRX.ExpenseReports.Client
{

  partial class ExpenseReportActions
  {

    public virtual void AddSupportingDocs(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var supDocs = Functions.ExpenseReport.Remote.GetFreeSupportingDocs(_obj).ShowSelectMany();
      foreach (var supDoc in supDocs)
      {
        var expense = _obj.Expenses.AddNew();
        expense.SupportingDoc = supDoc;
      }
    }

    public virtual bool CanAddSupportingDocs(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Заблокировать кнопку, если состояние != "Оформмление".
      return _obj.ExpenseReportStatus == ExpenseReport.ExpenseReportStatus.Start;
    }


    public virtual void CreateFromTemplateCustom(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.ExpenseReport.Remote.CreateFromTemplate(_obj);
      _obj.Edit();
    }

    public virtual bool CanCreateFromTemplateCustom(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Заблокировать кнопку, если карточка заблокирована кем-то другим или состояние != "Оформмление".
      var lockInfo = Locks.GetLockInfo(_obj);
      return lockInfo.IsLockedByMe && _obj.ExpenseReportStatus == ExpenseReport.ExpenseReportStatus.Start;
    }

    public override void CreateFromTemplate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateFromTemplate(e);
    }

    public override bool CanCreateFromTemplate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateFromTemplate(e);
    }

    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.State.IsChanged || _obj.State.IsInserted)
        _obj.Save();
      
      // Проверить, есть ли уже задачи в работе по этому авансовому отчету
      if (Functions.ExpenseReport.Remote.CheckExpenseReportApprovalTasksByDocument(_obj))
      {
        e.AddInformation(ExpenseReports.Resources.AlredySendedErrorText);
        return;
      }
      
      Functions.Module.Remote.CreateExpenseReportApprovalTask(_obj).Show();
      e.CloseFormAfterAction = true;
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
  }
}