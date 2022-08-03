using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.SupportingDocument;
using Sungero.Company;

namespace DirRX.ExpenseReports.Server
{
  partial class SupportingDocumentFunctions
  {
    
    /// <summary>
    /// Получить связанные авансовые отчеты
    /// </summary>
    [Remote(IsPure=true)]
    public IQueryable<IExpenseReport> GetLinkedExpenseReport()
    {
      return ExpenseReports.GetAll().Where(r => r.Expenses.Any(e => e.SupportingDoc.Equals(_obj)));
    }
    
    /// <summary>
    /// Посчитать количество страниц у версии документа, если она в формате PDF или WORD.
    /// </summary>
    /// <returns>Количество страниц, если формат поддерживается, иначе null.</returns>
    [Remote(IsPure=true)]
    public static Nullable<int> CountPages(Sungero.Content.IElectronicDocumentVersions version)
    {
      try
      {
        if (version.AssociatedApplication.Extension.ToLower() == "pdf")
        {
          var asposeDoc = Functions.Module.CreateAsposePdfDocument(version);
          return asposeDoc.Pages.Count;
        }
        else if (version.AssociatedApplication.Extension.ToLower() == "docx" ||
                 version.AssociatedApplication.Extension.ToLower() == "doc")
        {
          
          var asposeDoc = Functions.Module.CreateAsposeWordDocument(version);
          return asposeDoc.PageCount;
        }
        else
          return null;
      }
      catch (Exception e)
      {
        Logger.ErrorFormat("CountPages error: {0}", e.Message);
        return null;
      }
    }
  }
}