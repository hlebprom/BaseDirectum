using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Company;
using Sungero.Content;
using System.Globalization;

namespace DirRX.BusinessTrips.Server
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Получить тип расхода - Суточные.
    /// </summary>
    /// <returns>Тип расхода - Суточные.</returns>
    [Remote(IsPure = true), Public]
    public virtual DirRX.ExpenseReports.IExpenseType GetPerDiemExpenseType()
    {
      var expenseTypeId = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(DirRX.BusinessTrips.Constants.Module.DocflowParamKeys.PerDiemExpenseTypeIdKey);
      return DirRX.ExpenseReports.PublicFunctions.ExpenseType.Remote.GetExpenseTypeById(Convert.ToInt32(expenseTypeId));
    }
    
    /// <summary>
    /// Получить тип расхода - Проживание.
    /// </summary>
    /// <returns>Тип расхода - Проживание.</returns>
    [Remote(IsPure = true), Public]
    public virtual DirRX.ExpenseReports.IExpenseType GetHotelExpenseType()
    {
      var expenseTypeId = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(DirRX.BusinessTrips.Constants.Module.DocflowParamKeys.HotelExpenseTypeIdKey);
      return DirRX.ExpenseReports.PublicFunctions.ExpenseType.Remote.GetExpenseTypeById(Convert.ToInt32(expenseTypeId));
    }
    
    /// <summary>
    /// Получить подписанта приказов по командировке.
    /// </summary>
    /// <param name="businessUnit">Наша организация сотрудника.</param>
    /// <returns>Подписант приказов по командировке: роль или пользователь.</returns>
    [Public]
    public virtual IRecipient GetOrderSigner(IBusinessUnit businessUnit)
    {
      var orderSigner = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(Constants.Module.BusinessTripRoleGuids.OrderSigner, businessUnit);
      if (orderSigner != null)
        return orderSigner;
      // Если нет исполнителей роли, подписывать приказ будет ген. директор.
      else if (businessUnit != null)
        return businessUnit.CEO;
      return null;
    }
    
    /// <summary>
    /// Создать приказ по командировке с общими данными и связью с предыдущим приказом.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Созданный приказ с общими данными.</returns>
    [Public]
    public virtual IBusinessTripOrder CreateOrderBase(IBusinessTrip businessTrip, Guid orderKindEntityGuid, string orderTemplateName)
    {
      var template = Sungero.Content.ElectronicDocumentTemplates.GetAll(n => n.Name == orderTemplateName).FirstOrDefault();
      var document = BusinessTripOrders.CreateFrom(template);
      document.DocumentKind = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(orderKindEntityGuid);
      document.BusinessTrip = businessTrip;
      
      // Связать с предыдущим приказом на изменение или с исходным приказом
      var prevOrder = BusinessTripOrders.Null;
      var signedOrders = BusinessTripOrders.GetAll(o => Equals(o.BusinessTrip, businessTrip)
                                                   && o.InternalApprovalState == DirRX.BusinessTrips.BusinessTripOrder.InternalApprovalState.Signed);
      if (signedOrders.Count() > 1)
      {
        // Если по командировке уже есть несколько подписанных приказов, выбрать последний об изменении
        prevOrder = signedOrders.Where(o => Equals(o.DocumentKind, DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(DirRX.BusinessTrips.Constants.Module.DocumentKind.BusinessTripChangeOrderKind)))
          .OrderByDescending(o => o.Id)
          .FirstOrDefault();
      }
      else
      {
        // Иначе взять единственный подписанный, он же исходный
        if (signedOrders.Count() == 1)
          prevOrder = signedOrders.FirstOrDefault();
      }
      
      if (prevOrder != null)
      {
        prevOrder.Relations.Add(DirRX.BusinessTrips.PublicConstants.Module.CancelRelationName, document);
        document.OrderInfo = BusinessTripOrders.Resources.OrderInfoTemplateFormat(prevOrder.RegistrationNumber, prevOrder.RegistrationDate.Value.ToString("d"));
      }
      return document;
    }
    
    /// <summary>
    /// Создать приказ о направлении в командировку.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Созданный приказ о направлении в командировку.</returns>
    [Public]
    public virtual IBusinessTripOrder CreateOrder(IBusinessTrip businessTrip)
    {
      var document = CreateOrderBase(businessTrip,
                                     DirRX.BusinessTrips.Constants.Module.DocumentKind.BusinessTripOrderKind,
                                     DirRX.BusinessTrips.Constants.Module.TemplateNames.OrderTemplateName);
      document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Создать приказ об изменении командировки.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Созданный приказ об изменении командировки.</returns>
    [Public]
    public virtual IBusinessTripOrder CreateChangeOrder(IBusinessTrip businessTrip, string changeReason)
    {
      var document = CreateOrderBase(businessTrip,
                                     DirRX.BusinessTrips.Constants.Module.DocumentKind.BusinessTripChangeOrderKind,
                                     DirRX.BusinessTrips.Constants.Module.TemplateNames.ChangeTripOrderTemplateName);
      
      document.ChangeReason = changeReason;
      document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Создать приказ об отзыве из командировки.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Созданный приказ об отзыве из командировки.</returns>
    [Public]
    public virtual IBusinessTripOrder CreateRecallOrder(IBusinessTrip businessTrip, string recallReason, DateTime recallDate)
    {
      var document = CreateOrderBase(businessTrip,
                                     DirRX.BusinessTrips.Constants.Module.DocumentKind.BusinessTripRecallOrderKind,
                                     DirRX.BusinessTrips.Constants.Module.TemplateNames.RecallTripOrderTemplateName);
      
      document.RecallDate = recallDate;
      document.RecallReason = recallReason;
      document.Save();
      
      return document;
    }

    /// <summary>
    /// Создать приказ об отмене командировки.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Созданный приказ об отмене командировки.</returns>
    [Public]
    public virtual IBusinessTripOrder CreateCancelOrder(IBusinessTrip businessTrip, string cancelReason)
    {
      var document = CreateOrderBase(businessTrip,
                                     DirRX.BusinessTrips.Constants.Module.DocumentKind.BusinessTripCancelOrderKind,
                                     DirRX.BusinessTrips.Constants.Module.TemplateNames.CancelTripOrderTemplateName);
      
      document.CancelReason = cancelReason;
      document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Сформировать название командировки.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="departureDate">Дата отъезда.</param>
    /// <param name="returnDate">Дата возвращения.</param>
    /// <param name="purpose">Цель.</param>
    /// <returns>Название.</returns>
    [Remote]
    public virtual string FormatBusinessTripName(Sungero.Company.IEmployee employee, DateTime? departureDate, DateTime? returnDate, string purpose)
    {
      return Resources.BusinessTripNameTemplateFormat(employee != null ? employee.Name : string.Empty,
                                                      departureDate.HasValue ? departureDate.Value.ToString("d") : string.Empty,
                                                      returnDate.HasValue ? returnDate.Value.ToString("d") : string.Empty,
                                                      purpose);
    }

    /// <summary>
    /// Создать запись справочника Командировки.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="departureDate">Дата начала командировки.</param>
    /// <param name="returnDate">Дата окончания командировки.</param>
    /// <param name="purpose">Цель.</param>
    /// <param name="expencesSum">Сумма расходов.</param>
    /// <param name="financeSource">Источник финансирования.</param>
    /// <param name="byCar">Поездка на личном транспорте или нет.</param>
    /// <param name="needTicketAndHotel">Нужна или нет покупка билетов и оплата гостиницы.</param>
    /// <param name="carModel">Модель автомобиля - личного транспорта.</param>
    /// <param name="carNumber">Номер автомобиля - личного транспорта.</param>
    /// <param name="route">Маршрут командировки.</param>
    /// <param name="expenses">Расходы по командировке.</param>
    /// <param name="note">Примечание.</param>
    /// <returns>Созданная запись справочника Командировки.</returns>
    [Public]
    public virtual IBusinessTrip CreateBusinessTrip(IEmployee employee, DateTime departureDate, DateTime returnDate, string purpose, double expencesSum, string financeSource, bool byCar, bool needTicketAndHotel,
                                                    string carModel, string carNumber, List<Structures.BusinessTrip.IRouteStep> route, List<Structures.BusinessTrip.IExpense> expenses, string note)
    {
      var businessTrip = BusinessTrips.Create();
      businessTrip.Employee = employee;
      businessTrip.DepartureDate = departureDate;
      businessTrip.ReturnDate = returnDate;
      
      // Факт даты сразу заполнить исходными датами. Необходимо для корректрого формирования приказа об изменении из шаблона.
      businessTrip.FactDepartureDate = departureDate;
      businessTrip.FactReturnDate = returnDate;
      
      businessTrip.Purpose = purpose;
      businessTrip.ExpensesSum = expencesSum;
      businessTrip.FinanceSource = financeSource;
      businessTrip.ByCar = byCar;
      businessTrip.NeedTicketAndHotel = needTicketAndHotel;
      businessTrip.CarModel = carModel;
      businessTrip.CarNumber = carNumber;
      foreach (var taskRoutePoint in route)
      {
        var btRoutePoint = businessTrip.Route.AddNew();
        btRoutePoint.Destination = taskRoutePoint.Destination;
        btRoutePoint.Org = taskRoutePoint.Org;
        btRoutePoint.DateIn = taskRoutePoint.DateIn;
        btRoutePoint.DateOut = taskRoutePoint.DateOut;
      }
      businessTrip.Note = note;
      
      foreach (var expense in expenses)
      {
        var btExpense = businessTrip.Expenses.AddNew();
        btExpense.ExpenseType = expense.ExpenseType;
        btExpense.ExpenseSum = expense.ExpenseSum;
        btExpense.ExpenseDescription = expense.ExpenseDescription;
        btExpense.LimitSetting = expense.LimitSetting;
        btExpense.Limit = expense.Limit;
      }
      
      businessTrip.Save();
      
      return businessTrip;
    }
    
    /// <summary>
    /// Создать служебную записку о поездке на личном транспорте.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Служебная записка.</returns>
    public virtual IBusinessTripMemo CreateBusinessTripMemo(IBusinessTrip businessTrip)
    {
      var template = Sungero.Content.ElectronicDocumentTemplates.GetAll(n => n.Name == DirRX.BusinessTrips.Constants.Module.TemplateNames.MemoTemplateName).FirstOrDefault();
      var document = BusinessTripMemos.CreateFrom(template);
      
      document.DocumentKind = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(DirRX.BusinessTrips.Constants.Module.DocumentKind.BusinessTripMemoKind);
      document.BusinessTrip = businessTrip;
      document.LifeCycleState = DirRX.BusinessTrips.BusinessTripMemo.LifeCycleState.Active;
      document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Создать задачу на согласование командировки.
    /// </summary>
    /// <returns>Созданная задача на согласование командировки.</returns>
    [Remote(PackResultEntityEagerly = true), Public]
    public virtual IBusinessTripApprovalTask CreateNewBusinessTripApprovalTask()
    {
      return BusinessTripApprovalTasks.Create();
    }
    
    /// <summary>
    /// Получить все командировки текущего пользователя.
    /// </summary>
    /// <returns>Список командировок.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<IBusinessTrip> GetMyBusinessTrips()
    {
      return BusinessTrips.GetAll(t => Equals(t.Employee, Employees.Current));
    }
    
    /// <summary>
    /// Получить все предстоящие командировки текущего пользователя.
    /// </summary>
    /// <returns>Список командировок.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<IBusinessTrip> GetMyFutureBusinessTrips()
    {
      return BusinessTrips.GetAll(t => Equals(t.Employee, Employees.Current)
                                  && t.DepartureDate >= Sungero.Core.Calendar.Today
                                  && t.BusinessTripStatus != DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Finished
                                  && t.BusinessTripStatus != DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Canceled);
    }
    
    /// <summary>
    /// Получить все прошедшие командировки текущего пользователя.
    /// </summary>
    /// <returns>Список командировок.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<IBusinessTrip> GetMyPastBusinessTrips()
    {
      return BusinessTrips.GetAll(t => Equals(t.Employee, Employees.Current) && t.ReturnDate < Sungero.Core.Calendar.Today);
    }
    
    /// <summary>
    /// Получить задачи по отмене командировки.
    /// </summary>
    /// <param name="businessTripApprovalTask">Задача на согласование командировки.</param>
    /// <returns>Список задач по отмене командировки.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<IBusinessTripCancelTask> GetCancelTasksByBusinessTripApprovalTask(IBusinessTripApprovalTask businessTripApprovalTask)
    {
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(businessTripApprovalTask);
      return BusinessTripCancelTasks.GetAll(t => Equals(t.BusinessTrip, businessTrip));
    }
    
    /// <summary>
    /// Получить задачи по отзыву из командировки.
    /// </summary>
    /// <param name="businessTrip">Командировка.</param>
    /// <returns>Список задач по отзыву из командировки.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<IBusinessTripRecallTask> GetRecallTasksByBusinessTrip(IBusinessTrip businessTrip)
    {
      return BusinessTripRecallTasks.GetAll(t => Equals(t.BusinessTrip, businessTrip));
    }
  }
}