using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReport;

namespace DirRX.ExpenseReports
{
  partial class ExpenseReportExpensesClientHandlers
  {

    public virtual void ExpensesExpenseSumInCurrencyValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(ExpenseReports.Resources.SubzeroError);
    }

    public virtual void ExpensesExpenseSumValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(ExpenseReports.Resources.SubzeroError);
    }
  }

  partial class ExpenseReportClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      if (_obj.Expenses.Where(ex => ex.Limit.HasValue && ex.ExpenseSum.HasValue && ex.ExpenseSum.Value > ex.Limit.Value).Any())
        e.AddInformation(DirRX.ExpenseReports.ExpenseReports.Resources.LimitExceededErrorText);
    }

    public virtual void DocsCountValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(ExpenseReports.Resources.SubzeroError);
    }

    public virtual void PagesCountValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(ExpenseReports.Resources.SubzeroError);
    }

    public virtual void ApprovingSumValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(ExpenseReports.Resources.SubzeroError);
    }

    public virtual void GettedMoneyValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value < 0)
        e.AddError(ExpenseReports.Resources.SubzeroError);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      e.HideAction(_obj.Info.Actions.CreateFromFile);
      e.HideAction(_obj.Info.Actions.CreateFromTemplate);
      e.HideAction(_obj.Info.Actions.CreateFromScanner);
      
      // Скрыть панель регистрации.
      // Получить значение параметра, влияющего на отображение панели регистрации.
      bool showParamValue;
      var showParam = e.Params.TryGetValue(Sungero.Docflow.Constants.OfficialDocument.ShowParam, out showParamValue);
      // Выставить отрицательное значение параметра, чтобы при открытии формы скрывать панель регистрации.
      if (showParam)
        showParam = !showParamValue;
      // Сохранение отрицательного значения параметра.
      e.Params.AddOrUpdate(Sungero.Docflow.Constants.OfficialDocument.ShowParam, showParam);
      
      // Если Состояние отлично от "Оформмление", то заблокировать все поля.
      if (_obj.ExpenseReportStatus != ExpenseReport.ExpenseReportStatus.Start)
      {
        foreach(var property in _obj.State.Properties)
        {
          property.IsEnabled = false;
        }
        e.AddInformation(ExpenseReports.Resources.CantEditInfo);
      }
      
      foreach(var expenseRow in _obj.Expenses)
      {
        // Подсветить превышение лимитов.
        Functions.ExpenseReport.CheckLimit(_obj, expenseRow);
      }
      
      // Таблицу расходов сделать обязательной при показе, чтобы была возможность авто создания документа с путой таблицей.
      _obj.State.Properties.Expenses.IsRequired = true;
      
      // Тип расхода сделать обязательным при показе, чтобы была возможность заносить документы с почты
      _obj.State.Properties.Expenses.Properties.ExpenseType.IsRequired = true;
    }
  }

}