using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripApprovalTask;

namespace DirRX.BusinessTrips.Shared
{
  partial class BusinessTripApprovalTaskFunctions
  {
    /// <summary>
    /// Проверить и подсветить превышение лимита по строке расхода.
    /// </summary>
    /// <param name="expenseRow">Строка таблицы расходов.</param>
    public void CheckLimit(IBusinessTripApprovalTaskExpenses expenseRow)
    {
      if (expenseRow.ExpenseSum.HasValue && expenseRow.Limit.HasValue
          && expenseRow.ExpenseSum.Value > expenseRow.Limit.Value)
      {
        expenseRow.State.Properties.ExpenseSum.HighlightColor = Sungero.Core.Colors.Parse(DirRX.ExpenseReports.PublicConstants.Module.LimitHighlightColorRed);
        expenseRow.State.Properties.LimitSetting.HighlightColor = Sungero.Core.Colors.Parse(DirRX.ExpenseReports.PublicConstants.Module.LimitHighlightColorRed);
      }
      else
      {
        expenseRow.State.Properties.ExpenseSum.HighlightColor = Colors.Empty;
        expenseRow.State.Properties.LimitSetting.HighlightColor = Colors.Empty;
      }
    }
    
    /// <summary>
    /// Пересчитать сумму расходов.
    /// </summary>
    public void ReCountSum()
    {
      _obj.ExpensesSum = 0;
      // Сложить все суммы расходов
      foreach(var expanseRow in _obj.Expenses)
      {
        if (expanseRow.ExpenseSum.HasValue)
          _obj.ExpensesSum += expanseRow.ExpenseSum.Value;
      }
    }
    
    /// <summary>
    /// Установить доступность и обязательность полей: "Марка автомобиля" и "Рег. номер"
    /// </summary>
    public void ProcessCarProperties()
    {
      // Если установлен чекбокс "Оформить поездку на личном автомобиле", то поля доступны и обязательны, иначе нет
      var byCar = false;
      if (_obj.ByCar.HasValue)
        byCar = _obj.ByCar.Value;

      _obj.State.Properties.CarModel.IsEnabled = byCar;
      _obj.State.Properties.CarModel.IsRequired = byCar;
      
      _obj.State.Properties.CarNumber.IsEnabled = byCar;
      _obj.State.Properties.CarNumber.IsRequired = byCar;
    }

  }
}