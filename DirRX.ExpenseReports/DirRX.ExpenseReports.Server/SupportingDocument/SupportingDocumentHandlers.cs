using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.SupportingDocument;

namespace DirRX.ExpenseReports
{
  partial class SupportingDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.Employee = Sungero.Company.Employees.Current;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Пересчитать страницы, если последняя версия изменилась
      if (_obj.HasVersions && _obj.LastVersion.State.IsChanged)
        _obj.PagesCount = Functions.SupportingDocument.CountPages(_obj.LastVersion);
    }
  }

}