using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.BusinessTrips.Structures.BusinessTrip
{

  /// <summary>
  /// Этап маршрута командировки.
  /// </summary>
  [Public]
  partial class RouteStep
  {
    /// <summary>
    /// Город.
    /// </summary>
    public Sungero.Commons.ICity Destination { get;set; }
    
    /// <summary>
    /// Организация.
    /// </summary>
    public Sungero.Parties.ICompany Org { get;set; }
    
    /// <summary>
    /// Дата прибытия.
    /// </summary>
    public DateTime DateIn { get;set; }
    
    /// <summary>
    /// Дата отъезда.
    /// </summary>
    public DateTime DateOut { get;set; }
  }
  
  /// <summary>
  /// Строка расхода по командировке.
  /// </summary>
  [Public]
  partial class Expense
  {
    /// <summary>
    /// Тип расхода.
    /// </summary>
    public DirRX.ExpenseReports.IExpenseType ExpenseType { get;set; }
    
    /// <summary>
    /// Сумма.
    /// </summary>
    public double ExpenseSum { get;set; }
    
    /// <summary>
    /// Описание расхода.
    /// </summary>
    public String ExpenseDescription { get;set; }
    
    /// <summary>
    /// Настройка лимита.
    /// </summary>
    public DirRX.ExpenseReports.ILimitSetting LimitSetting { get;set; }
    
    /// <summary>
    /// Лимит с учетом количества дней.
    /// </summary>
    public double? Limit { get;set; }
    
  }

}