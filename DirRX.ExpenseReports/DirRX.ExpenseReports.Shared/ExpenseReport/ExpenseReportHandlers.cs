using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseReport;
using System.Globalization;
using CommonLibrary;

namespace DirRX.ExpenseReports
{
  partial class ExpenseReportExpensesSharedCollectionHandlers
  {

    public virtual void ExpensesDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      Functions.ExpenseReport.ReCountSums(_obj);
      Functions.ExpenseReport.ReCountDocsAndPages(_obj);
    }
  }

  partial class ExpenseReportExpensesSharedHandlers
  {

    public virtual void ExpensesExpenseTypeChanged(DirRX.ExpenseReports.Shared.ExpenseReportExpensesExpenseTypeChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.LimitSetting = DirRX.ExpenseReports.Functions.Module.Remote.GetLimitSetting(e.NewValue, _obj.ExpenseReport.Employee, Sungero.Commons.Cities.Null);
    }

    public virtual void ExpensesLimitSettingChanged(DirRX.ExpenseReports.Shared.ExpenseReportExpensesLimitSettingChangedEventArgs e)
    {
      if (e.NewValue != null)
        _obj.Limit = e.NewValue.Limit;
    }

    public virtual void ExpensesExpenseDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      _obj.ExpenseSum = Functions.Module.ConvertIntoRUB(_obj.Currency, _obj.ExpenseSumInCurrency, _obj.ExpenseDate);
    }

    public virtual void ExpensesCurrencyChanged(DirRX.ExpenseReports.Shared.ExpenseReportExpensesCurrencyChangedEventArgs e)
    {
      _obj.ExpenseSum = Functions.Module.ConvertIntoRUB(_obj.Currency, _obj.ExpenseSumInCurrency, _obj.ExpenseDate);
    }

    public virtual void ExpensesExpenseSumInCurrencyChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      _obj.ExpenseSum = Functions.Module.ConvertIntoRUB(_obj.Currency, _obj.ExpenseSumInCurrency, _obj.ExpenseDate);
    }

    public virtual void ExpensesSupportingDocChanged(DirRX.ExpenseReports.Shared.ExpenseReportExpensesSupportingDocChangedEventArgs e)
    {
      Functions.ExpenseReport.ReCountDocsAndPages(_obj.ExpenseReport);
      
      if (e.NewValue != null)
      {
        if (!_obj.ExpenseDate.HasValue)
          _obj.ExpenseDate = e.NewValue.Date;
        
        if (string.IsNullOrEmpty(_obj.ExpenseDescription))
          _obj.ExpenseDescription = e.NewValue.Subject;
        
        if (!_obj.ExpenseSumInCurrency.HasValue)
          _obj.ExpenseSumInCurrency = e.NewValue.Sum;
        
        if (_obj.Currency == null)
          _obj.Currency = e.NewValue.Currency;
      }
    }

    public virtual void ExpensesExpenseSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ExpenseReport.ReCountSums(_obj.ExpenseReport);
      Functions.ExpenseReport.CheckLimit(_obj.ExpenseReport, _obj);
    }
  }

  partial class ExpenseReportSharedHandlers
  {

    public virtual void ApprovingSumChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      // Подготовить данные для вставки в шаблон (Израсхородано): 1000.00
      _obj.ExpensesMoneyForTemplate = e.NewValue.Value.ToString("F", CultureInfo.InvariantCulture);
      
      if (e.NewValue.HasValue)
      {
        // Подготовить данные для вставки в шаблон (к утверждению): рубли и копейки отдельно
        var approvedSumStr = e.NewValue.Value.ToString("F", CultureInfo.InvariantCulture);
        var parts = approvedSumStr.Split('.');
        _obj.ApprovedSumRub = parts[0];
        _obj.ApprovedSumCent = parts[1];
        
        // Сгенерировать сумму прописью
        _obj.ApprovedSumRubInLetters = StringUtils.NumberToWords((long)Math.Truncate(e.NewValue.Value));
      }
    }

    public virtual void OverrunMoneyChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      // Подготовить данные для вставки в шаблон: 1000.00
      _obj.OverrunMoneyForTemplate = e.NewValue.Value.ToString("F", CultureInfo.InvariantCulture);
    }

    public virtual void RemainMoneyChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      // Подготовить данные для вставки в шаблон: 1000.00
      _obj.RemainMoneyForTemplate = e.NewValue.Value.ToString("F", CultureInfo.InvariantCulture);
    }

    public virtual void EmployeeChanged(DirRX.ExpenseReports.Shared.ExpenseReportEmployeeChangedEventArgs e)
    {
      if (e.NewValue != null)
      {
        _obj.BusinessUnit = e.NewValue.Department.BusinessUnit;
        _obj.Department = e.NewValue.Department;
      }
      else
      {
        _obj.BusinessUnit = null;
        _obj.Department = null;
      }
    }

    public virtual void GettedMoneyChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      Functions.ExpenseReport.ReCountSums(_obj);
      
      // Подготовить данные для вставки в шаблон: 1000.00
      if (e.NewValue.HasValue)
        _obj.GettedMoneyForTemplate = e.NewValue.Value.ToString("F", CultureInfo.InvariantCulture);
      
    }

  }
}