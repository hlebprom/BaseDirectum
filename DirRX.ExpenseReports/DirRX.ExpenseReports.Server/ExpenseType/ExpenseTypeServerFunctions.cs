using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.ExpenseType;

namespace DirRX.ExpenseReports.Server
{
  partial class ExpenseTypeFunctions
  {
    
    /// <summary>
    /// Получить Тип расходов по наименованию.
    /// </summary>
    /// <param name="name">Имя записи.</param>
    /// <returns>Запись справочника Типов расходов.</returns>
    [Public]
    public static DirRX.ExpenseReports.IExpenseType CreateAndGetExpenseType(string name)
    {
      var expenseType = DirRX.ExpenseReports.ExpenseTypes.GetAll(t => t.Name == name).FirstOrDefault();
      if (expenseType == null)
      {
        expenseType = ExpenseTypes.Create();
        expenseType.Name = name;
        expenseType.Save();
      }
      return expenseType;
    }

    /// <summary>
    /// Получить тип расхода по ИД.
    /// </summary>
    /// <param name="id">ИД записи.</param>
    /// <returns>Запись справочника Типов расходов.</returns>
    [Remote(IsPure = true), Public]
    public static DirRX.ExpenseReports.IExpenseType GetExpenseTypeById(int id)
    {
      return DirRX.ExpenseReports.ExpenseTypes.GetAll(t => t.Id == id).FirstOrDefault();
    }

  }
}