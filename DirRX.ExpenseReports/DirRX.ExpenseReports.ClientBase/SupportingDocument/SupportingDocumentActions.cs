using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.SupportingDocument;

namespace DirRX.ExpenseReports.Client
{

  partial class SupportingDocumentActions
  {

    public virtual void ShowExpenseReports(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.SupportingDocument.Remote.GetLinkedExpenseReport(_obj).Show();
    }

    public virtual bool CanShowExpenseReports(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}