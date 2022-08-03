using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReportPrepareAssignment;

namespace DirRX.ExpenseReports.Server
{
  partial class ExpenseReportPrepareAssignmentFunctions
  {
    
    /// <summary>
    /// Построить модель состояния с инструкцией.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public virtual StateView GetPrepareExpenseReportAssignmentState()
    {
      return Functions.Module.GetTextState(
        ExpenseReportPrepareAssignments.Resources.HowToFillExpenseReportInstructionFormat(
          Functions.Module.GetDocflowParamValue(Constants.Module.DocflowParamKeys.SupportingDocsMailKey)));;
    }

  }
}