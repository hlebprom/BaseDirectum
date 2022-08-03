using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.BusinessTrips.BusinessTripApprovalTask;
using BusinessTripRoleGuids = DirRX.BusinessTrips.Constants.Module.BusinessTripRoleGuids;
using System.Globalization;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripApprovalTaskRouteHandlers
  {
    #region Требуется выдача денег?
    
    public virtual bool Decision58Result()
    {
      // Выдача денег требуется, если деньги ещё не выдавались или сумма увеличилась после выдачи
      if (// Задания на выдачу денег ещё не было
          _obj.GiveMoneyAssignment == null
          || // Задание на выдачу денег не было выполнено
          _obj.GiveMoneyAssignment != null
          && _obj.GiveMoneyAssignment.Result != DirRX.BusinessTrips.BusinessTripSimpleAssignment.Result.Complete
          || // Сумма увеличилась после выдачи денег и изменения командировки
          _obj.IsChangedByUser == true
          && _obj.GiveMoneyAssignment != null
          && _obj.GiveMoneyAssignment.Result == DirRX.BusinessTrips.BusinessTripSimpleAssignment.Result.Complete
          && _obj.OldExpensesSum.Value < _obj.ExpensesSum.Value)
        return true;
      else
        return false;
    }

    #endregion
    
    #region Старт задачи на подпись приказа об изменении
    
    public virtual void Script57Execute()
    {
      // Если факт даты отличаются от исходных, то создать задачу на подпись приказа об изменении
      var returnDate = _obj.ReturnDate;
      // Если был отзыв, то вместо плановой даты окончания взять дату отзыва, чтобы в этом случае приказ об изменении не создавался
      if (_obj.RecallDate.HasValue)
        returnDate = _obj.RecallDate;
      
      
      if (_obj.DepartureDate != _obj.FactDepartureDate || returnDate != _obj.FactReturnDate)
      {
        var changeOrderTask = BusinessTripChangeOrderTasks.Create();
        changeOrderTask.ChangeReason = _obj.ChangeReason;
        changeOrderTask.BusinessTripApprovalTask = _obj;
        changeOrderTask.Start();
      }
    }
    
    #endregion

    #region Удаление служебной записки
    
    public virtual void Script56Execute()
    {
      // Удалить служебку. После доработки она сгенерируется снова
      Functions.BusinessTripApprovalTask.DeleteMemo(_obj);
    }
    
    #endregion
    
    #region Требуется изменение командировки?
    
    public virtual bool Decision55Result()
    {
      return _obj.NeedChange.Value;
    }

    #endregion
    
    #region Требуется изменение командировки?
    
    public virtual bool Decision53Result()
    {
      return _obj.NeedChange.Value;
    }

    #endregion
    
    #region Подготовка авансового отчета
    
    public virtual void StartBlock52(DirRX.BusinessTrips.Server.BusinessTripPrepareReportAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      var expenseReport = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      e.Block.Subject = BusinessTripApprovalTasks.Resources.PrepareExpenseReportSubjectTemplateFormat(expenseReport.RegistrationNumber ?? String.Empty,
                                                                                                      expenseReport.RegistrationDate.HasValue ? expenseReport.RegistrationDate.Value.ToString("d") : String.Empty,
                                                                                                      expenseReport.Purpose);
      
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.PrepareReport;
    }
    
    public virtual void StartAssignment52(DirRX.BusinessTrips.IBusinessTripPrepareReportAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripPrepareReportAssignmentArguments e)
    {
      // Скопировать значения свойств из задачи
      assignment.FactDepartureDate = _obj.DepartureDate;
      assignment.GettedMoney = _obj.ExpensesSum;

      assignment.Route.Clear();
      foreach(var routePoint in _obj.Route)
      {
        var blockRoutePoint = assignment.Route.AddNew();
        blockRoutePoint.Org = routePoint.Org;
        blockRoutePoint.Destination = routePoint.Destination;
        blockRoutePoint.DateIn = routePoint.DateIn;
        blockRoutePoint.DateOut = routePoint.DateOut;
      }
    }
    
    public virtual void CompleteAssignment52(DirRX.BusinessTrips.IBusinessTripPrepareReportAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripPrepareReportAssignmentArguments e)
    {
      _obj.FactDepartureDate = assignment.FactDepartureDate;
      _obj.FactReturnDate = assignment.FactReturnDate;
      
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      var expenseReport = businessTrip.ExpenseReport;

      // Синхронизировать расходы в авансовый отчет
      expenseReport.GettedMoney = assignment.GettedMoney;
      expenseReport.Expenses.Clear();
      foreach(var assignmentExpense in assignment.Expenses)
      {
        var docExpense = expenseReport.Expenses.AddNew();
        docExpense.SupportingDoc = assignmentExpense.SupportingDoc;
        docExpense.ExpenseDescription = assignmentExpense.ExpenseDescription;
        docExpense.ExpenseDate = assignmentExpense.ExpenseDate;
        docExpense.ExpenseSumInCurrency = assignmentExpense.ExpenseSumInCurrency;
        docExpense.Currency = assignmentExpense.Currency;
        docExpense.ExpenseSum = assignmentExpense.ExpenseSum;
        docExpense.ExpenseNumber = assignmentExpense.ExpenseNumber;
        docExpense.ExpenseType = assignmentExpense.ExpenseType;
        docExpense.LimitSetting = assignmentExpense.LimitSetting;
        docExpense.Limit = assignmentExpense.Limit;
      }
      expenseReport.ApprovingSum = assignment.ApprovingSum;
      expenseReport.DocsCount = assignment.DocsCount;
      expenseReport.PagesCount = assignment.PagesCount;
      
      // Указать Факт дату как дату регистрации АО (требование законодательства)
      expenseReport.RegistrationDate = _obj.FactReturnDate;
      expenseReport.Save();
      
      // Синхронизировать факт даты в командировку
      businessTrip.FactDepartureDate = _obj.FactDepartureDate;
      businessTrip.FactReturnDate = _obj.FactReturnDate;
      
      businessTrip.Route.Clear();
      foreach(var assignmentRouteStep in assignment.Route)
      {
        var routeStep = businessTrip.Route.AddNew();
        routeStep.Destination = assignmentRouteStep.Destination;
        routeStep.Org = assignmentRouteStep.Org;
        routeStep.DateIn = assignmentRouteStep.DateIn;
        routeStep.DateOut = assignmentRouteStep.DateOut;
        routeStep.FactDateIn = assignmentRouteStep.FactDateIn;
        routeStep.FactDateOut = assignmentRouteStep.FactDateOut;
      }
      businessTrip.Save();
    }
    
    #endregion
    
    #region Последний день командировки?
    
    public virtual bool Decision51Result()
    {
      // Проверка нужна, чтобы в последний день не присылать сотруднику и уведомление и задание
      return _obj.ReturnDate.Value <= Sungero.Core.Calendar.Today
        || DirRX.ExpenseReports.PublicFunctions.Module.Remote.IsDebugEnabled();
    }
    
    #endregion
    
    #region Напоминание о заполнении авансового отчета
    
    public virtual void Script50Execute()
    {
      
      // Отправить напоминание скриптом, а не отдельным блоком, чтобы инструкция не терялась в переписке основной задачи
      var notification = Sungero.Workflow.SimpleTasks.CreateAsSubtask(_obj);
      notification.Subject = BusinessTripApprovalTasks.Resources.ExpenseReportNoticeJobNoticeSubjectFormat(_obj.DepartureDate.Value.ToString("d"),
                                                                                                           _obj.ReturnDate.Value.ToString("d"),
                                                                                                           _obj.Purpose);
      
      notification.ActiveText = BusinessTripApprovalTasks.Resources.HowToFillExpenseReportNotificationTextFormat(
        DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(
          DirRX.ExpenseReports.PublicConstants.Module.DocflowParamKeys.SupportingDocsMailKey));
      
      var notificationRoute = notification.RouteSteps.AddNew();
      notificationRoute.AssignmentType = Sungero.Workflow.SimpleTaskRouteSteps.AssignmentType.Notice;
      notificationRoute.Performer = _obj.Employee;
      notification.NeedsReview = false;
      
      notification.Start();
    }

    #endregion
    
    #region Удаление приказа
    
    public virtual void Script49Execute()
    {
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      // Удалить последний неподписанный приказ по командировке
      // HACK .Last() не заработал, поэтому использую обратную сортировку по Id и .FirstOrDefault()
      var order = BusinessTripOrders.GetAll(o => Equals(o.BusinessTrip, businessTrip)).OrderByDescending(o => o.Id).FirstOrDefault();
      if (order != null && order.InternalApprovalState != DirRX.BusinessTrips.BusinessTripOrder.InternalApprovalState.Signed)
        DirRX.BusinessTrips.BusinessTripOrders.Delete(order);
    }
    
    #endregion

    #region Ожидание дня отправки напоминания о заполнении авансового отчета
    
    public virtual bool Monitoring46Result()
    {
      return _obj.DepartureDate.Value <= Sungero.Core.Calendar.Today.AddDays(Constants.Module.ExpenseReportRemindMessageDelayDays)
        || DirRX.ExpenseReports.PublicFunctions.Module.Remote.IsDebugEnabled() // В режиме отладки мониторинг пропускается
        || _obj.NeedChange.Value; // TODO Период мониторинга 1 раз в день. Если сотрудник инициирует изменение, то задание на доработку может прийти не скоро. Красивого решения пока нет.
    }

    #endregion
    
    #region Старт задачи на согласование авансового отчета
    
    public virtual void Script45Execute()
    {
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);

      // Формирование задачи.
      var expRepTask = DirRX.ExpenseReports.ExpenseReportApprovalTasks.Create();
      expRepTask.ExpenseReportGroup.ExpenseReports.Add(businessTrip.ExpenseReport);
      expRepTask.OtherGroup.All.Add(businessTrip);
      expRepTask.NeedPrepare = false;
      
      expRepTask.Start();
      
      businessTrip.ExpenseReportApprovalTaskId = expRepTask.Id;
      
      businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Finished;
      
      businessTrip.Save();
    }
    
    #endregion
    
    #region Ожидание последнего дня командировки
    
    public virtual bool Monitoring44Result()
    {
      return _obj.ReturnDate.Value <= Sungero.Core.Calendar.Today
        || DirRX.ExpenseReports.PublicFunctions.Module.Remote.IsDebugEnabled() // В режиме отладки мониторинг пропускается
        || _obj.NeedChange.Value; // TODO Период мониторинга 1 раз в день. Если сотрудник инициирует изменение, то задание на доработку может прийти не скоро. Красивого решения пока нет.
    }

    #endregion

    #region Согласование суммы расходов

    public virtual void StartBlock42(DirRX.BusinessTrips.Server.BusinessTripApprovalAssignmentArguments e)
    {
      var accountant = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.Accountant,
                                                                                                   Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(accountant);
      Functions.BusinessTripApprovalTask.AddRecipientForAbortNotification(_obj, accountant);
      
      if (_obj.IsChangedByUser == true)
        e.Block.Subject = BusinessTripApprovalTasks.Resources.ChangeApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
      else
        e.Block.Subject = BusinessTripApprovalTasks.Resources.ApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    #endregion
    
    #region Требуется изменение командировки?
    
    public virtual bool Decision31Result()
    {
      return _obj.NeedChange.Value;
    }
    
    #endregion
    
    #region Создание записи справочника Командировки (Блок 26)

    public virtual void Script26Execute()
    {
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      if (businessTrip == null)
      {
        var route = new List<Structures.BusinessTrip.IRouteStep> ();
        foreach (var routePoint in _obj.Route)
        {
          var routeStructure = Structures.BusinessTrip.RouteStep.Create();
          routeStructure.Destination = routePoint.Destination;
          routeStructure.Org = routePoint.Org;
          routeStructure.DateIn = routePoint.DateIn.Value;
          routeStructure.DateOut = routePoint.DateOut.Value;
          route.Add(routeStructure);
        }
        
        var expenses = new List<Structures.BusinessTrip.IExpense> ();
        foreach (var businessTripExpense in _obj.Expenses)
        {
          var expenseStructure = Structures.BusinessTrip.Expense.Create();
          expenseStructure.ExpenseType = businessTripExpense.ExpenseType;
          expenseStructure.ExpenseSum = businessTripExpense.ExpenseSum.Value;
          expenseStructure.ExpenseDescription = businessTripExpense.ExpenseDescription;
          expenseStructure.LimitSetting = businessTripExpense.LimitSetting;
          if (businessTripExpense.Limit.HasValue)
            expenseStructure.Limit = businessTripExpense.Limit.Value;
          
          expenses.Add(expenseStructure);
        }
        
        businessTrip = Functions.Module.CreateBusinessTrip(_obj.Employee, _obj.DepartureDate.Value, _obj.ReturnDate.Value, _obj.Purpose, _obj.ExpensesSum.Value,
                                                           !string.IsNullOrEmpty(_obj.FinanceSource) ? _obj.FinanceSource : " ",
                                                           _obj.ByCar.Value, _obj.NeedTicketAndHotel.Value, _obj.CarModel, _obj.CarNumber, route, expenses, _obj.StartTaskText);
        
        businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.OnApproval;
        
        _obj.BusinessTripGroup.All.Add(businessTrip);
        _obj.BusinessTripId = businessTrip.Id;
      }
    }
    
    #endregion

    #region Создание служебной записки (Блок 30)
    
    public virtual void Script30Execute()
    {
      if (_obj.ByCar.Value)
      {
        var document = Functions.Module.CreateBusinessTripMemo(Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj));

        // Сконвертировать в PDF
        DirRX.ExpenseReports.PublicFunctions.Module.ConvertLastVersionToPDF(document);
        
        _obj.BusinessTripMemo = document;
      }
    }
    
    #endregion

    #region Согласование с руководителем (блок 3)
    
    public virtual void StartBlock3(DirRX.BusinessTrips.Server.BusinessTripApprovalAssignmentArguments e)
    {
      var manager = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetManager(_obj.Employee);
      if (manager != null)
      {
        e.Block.Performers.Add(manager);
        _obj.BossApprovalPerformer = manager;
        
        if (_obj.IsChangedByUser == true)
          e.Block.Subject = BusinessTripApprovalTasks.Resources.ChangeApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
        else
          e.Block.Subject = BusinessTripApprovalTasks.Resources.ApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
        
        Functions.BusinessTripApprovalTask.AddRecipientForAbortNotification(_obj, manager);
      }
    }
    
    public virtual void StartAssignment3(DirRX.BusinessTrips.IBusinessTripApprovalAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripApprovalAssignmentArguments e)
    {
      assignment.NeedShowCancelTripButton = true;
    }
    
    #endregion
    
    #region Корректировка параметров командировки инициатором (блок 9)
    
    public virtual void StartBlock9(DirRX.BusinessTrips.Server.BusinessTripReWorkAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = BusinessTripApprovalTasks.Resources.ReWorkAssignmentSubjectTemplateFormat(_obj.DepartureDate.Value.ToString("d"), _obj.ReturnDate.Value.ToString("d"), _obj.Purpose);
      
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Started;
    }
    
    public virtual void StartAssignment9(DirRX.BusinessTrips.IBusinessTripReWorkAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripReWorkAssignmentArguments e)
    {
      // Скопировать значения свойств из задачи
      assignment.Employee = _obj.Employee;
      assignment.Purpose = _obj.Purpose;
      assignment.DepartureDate = _obj.DepartureDate;
      assignment.ReturnDate = _obj.ReturnDate;
      assignment.FinanceSource = _obj.FinanceSource;

      assignment.Route.Clear();
      foreach(var routePoint in _obj.Route)
      {
        var blockRoutePoint = assignment.Route.AddNew();
        blockRoutePoint.Org = routePoint.Org;
        blockRoutePoint.Destination = routePoint.Destination;
        blockRoutePoint.DateIn = routePoint.DateIn;
        blockRoutePoint.DateOut = routePoint.DateOut;
      }
      assignment.ByCar = _obj.ByCar;
      assignment.CarModel = _obj.CarModel;
      assignment.CarNumber = _obj.CarNumber;
      
      assignment.Expenses.Clear();
      foreach(var expense in _obj.Expenses)
      {
        var blockExpense = assignment.Expenses.AddNew();
        blockExpense.ExpenseType = expense.ExpenseType;
        blockExpense.ExpenseSum = expense.ExpenseSum;
        blockExpense.ExpenseDescription = expense.ExpenseDescription;
        blockExpense.LimitSetting = expense.LimitSetting;
        blockExpense.Limit = expense.Limit;
      }
      assignment.NeedTicketAndHotel = _obj.NeedTicketAndHotel;
      
      // Запомнить старую сумму расходов
      _obj.OldExpensesSum = _obj.ExpensesSum;
    }
    
    public virtual void CompleteAssignment9(DirRX.BusinessTrips.IBusinessTripReWorkAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripReWorkAssignmentArguments e)
    {
      _obj.NeedChange = false;
      
      #region Синхронизировать свойства в задачу
      _obj.Purpose = assignment.Purpose;
      _obj.DepartureDate = assignment.DepartureDate;
      _obj.ReturnDate = assignment.ReturnDate;
      _obj.FinanceSource = assignment.FinanceSource;
      
      
      _obj.Route.Clear();
      foreach(var blockRoutePoint in assignment.Route)
      {
        var routePoint = _obj.Route.AddNew();
        routePoint.Org = blockRoutePoint.Org;
        routePoint.Destination = blockRoutePoint.Destination;
        routePoint.DateIn = blockRoutePoint.DateIn;
        routePoint.DateOut = blockRoutePoint.DateOut;
      }
      _obj.ByCar = assignment.ByCar;
      _obj.CarModel = assignment.CarModel;
      _obj.CarNumber = assignment.CarNumber;
      
      _obj.Expenses.Clear();
      foreach(var blockExpense in assignment.Expenses)
      {
        var expense = _obj.Expenses.AddNew();
        expense.ExpenseType = blockExpense.ExpenseType;
        expense.ExpenseSum = blockExpense.ExpenseSum;
        expense.ExpenseDescription = blockExpense.ExpenseDescription;
        expense.LimitSetting = blockExpense.LimitSetting;
        expense.Limit = blockExpense.Limit;
      }
      _obj.NeedTicketAndHotel = assignment.NeedTicketAndHotel;
      #endregion
      
      #region Синхронизировать свойства в командировку
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      businessTrip.Purpose = assignment.Purpose;
      businessTrip.DepartureDate = assignment.DepartureDate;
      businessTrip.ReturnDate = assignment.ReturnDate;
      
      // Факт даты сразу заполнить исходными датами. Необходимо для корректрого формирования приказа об изменении из шаблона.
      businessTrip.FactDepartureDate = assignment.DepartureDate;
      businessTrip.FactReturnDate = assignment.ReturnDate;
      businessTrip.FinanceSource = assignment.FinanceSource;
      
      businessTrip.Route.Clear();
      foreach (var taskRoutePoint in assignment.Route)
      {
        var btRoutePoint = businessTrip.Route.AddNew();
        btRoutePoint.Destination = taskRoutePoint.Destination;
        btRoutePoint.Org = taskRoutePoint.Org;
        btRoutePoint.DateIn = taskRoutePoint.DateIn;
        btRoutePoint.DateOut = taskRoutePoint.DateOut;
      }
      businessTrip.ByCar = assignment.ByCar;
      businessTrip.CarModel = assignment.CarModel;
      businessTrip.CarNumber = assignment.CarNumber;
      
      businessTrip.Expenses.Clear();
      foreach(var blockExpense in assignment.Expenses)
      {
        var expense = businessTrip.Expenses.AddNew();
        expense.ExpenseType = blockExpense.ExpenseType;
        expense.ExpenseSum = blockExpense.ExpenseSum;
        expense.ExpenseDescription = blockExpense.ExpenseDescription;
        expense.LimitSetting = blockExpense.LimitSetting;
        expense.Limit = blockExpense.Limit;
      }
      businessTrip.ExpensesSum = assignment.ExpensesSum;
      businessTrip.NeedTicketAndHotel = assignment.NeedTicketAndHotel;
      
      businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.OnApproval;
      businessTrip.Save();
      #endregion
      
      // Переформировать тему и текст задачи, т.к. данные командировки могли поменяться.
      Functions.BusinessTripApprovalTask.UpdateTaskSubject(_obj);
      Functions.BusinessTripApprovalTask.UpdateTaskText(_obj);
    }
    
    #endregion
    
    #region Создание приказа (блок 4)
    
    public virtual void Script4Execute()
    {
      var trip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      if (!_obj.OrderGroup.BusinessTripOrders.Any())
        _obj.OrderGroup.BusinessTripOrders.Add(PublicFunctions.Module.CreateOrder(trip));
      else
      {
        _obj.OtherGroup.All.Add(PublicFunctions.Module.CreateChangeOrder(trip, _obj.ChangeReason));
        _obj.ChangeOrderExists = true;
      }
    }
    
    #endregion
    
    #region Подписание приказа (блок 5)
    
    public virtual void StartBlock5(DirRX.BusinessTrips.Server.BusinessTripSignOrderAssignmentArguments e)
    {
      var orderSigner = Functions.Module.GetOrderSigner(Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));

      if (orderSigner != null)
      {
        e.Block.Performers.Add(orderSigner);
        Functions.BusinessTripApprovalTask.AddRecipientForAbortNotification(_obj, orderSigner);
        _obj.OrderSignerPerformer = orderSigner;

        if (_obj.ChangeOrderExists == true)
          e.Block.Subject = BusinessTripApprovalTasks.Resources.SignChangeOrderSubjectTemplateFormat(_obj.CommonSubjectPart);
        else
          e.Block.Subject = BusinessTripApprovalTasks.Resources.SignOrderSubjectTemplateFormat(_obj.CommonSubjectPart);
        
        _obj.OrderSigned = false;
      }
      
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.OrderPendingSign;
    }
    
    #endregion
    
    #region Требуется покупка билетов? (блок 16)
    
    public virtual bool Decision16Result()
    {
      return _obj.NeedTicketAndHotel.Value;
    }
    
    #endregion
    
    #region Покупка билетов (блок 17)
    
    public virtual void StartBlock17(DirRX.BusinessTrips.Server.BusinessTripSimpleAssignmentArguments e)
    {
      var tiketsResponsible = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.TiketsResponsible,
                                                                                                          Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(tiketsResponsible);
      Functions.BusinessTripApprovalTask.AddRecipientForAbortNotification(_obj, tiketsResponsible);

      if (_obj.IsChangedByUser == true
          && _obj.BuyTicketsAssignment != null
          && _obj.BuyTicketsAssignment.Result == DirRX.BusinessTrips.BusinessTripSimpleAssignment.Result.Complete)

        e.Block.Subject = BusinessTripApprovalTasks.Resources.ChangeBuyTiketsSubjectTemplateFormat(_obj.CommonSubjectPart);
      else
        e.Block.Subject = BusinessTripApprovalTasks.Resources.BuyTiketsSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void StartAssignment17(DirRX.BusinessTrips.IBusinessTripSimpleAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripSimpleAssignmentArguments e)
    {
      _obj.BuyTicketsAssignment = assignment;
    }
    
    #endregion
    
    #region Билеты куплены (блок 19)
    
    public virtual void StartBlock19(Sungero.Workflow.Server.NoticeArguments e)
    {
      // Уведомление о покупке билетов
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = BusinessTripApprovalTasks.Resources.TiketsNoticeSubjectTemplateFormat(_obj.DepartureDate.Value.ToString("d"), _obj.ReturnDate.Value.ToString("d"), _obj.Purpose);
      
      _obj.TicketAndHotelProccessed = true;
    }
    
    #endregion

    #region Ознакомление с приказом (блок 21)
    
    public virtual void StartBlock21(DirRX.BusinessTrips.Server.BusinessTripProcessDocumentsAssignmentArguments e)
    {
      _obj.OrderSigned = true;
      
      e.Block.Performers.Add(_obj.Employee);
      
      // Тема зависит от наличия служебной записки и от согласования\изменения
      if (_obj.ChangeOrderExists == true)
      {
        if (_obj.BusinessTripMemo != null)
          e.Block.Subject = BusinessTripApprovalTasks.Resources.ProcessChangeDocumentsWithMemoSubjectTemplateFormat(_obj.CommonSubjectPart);
        else
          e.Block.Subject = BusinessTripApprovalTasks.Resources.ProcessChangeDocumentsSubjectTemplateFormat(_obj.CommonSubjectPart);
      }
      else
      {
        if (_obj.BusinessTripMemo != null)
          e.Block.Subject = BusinessTripApprovalTasks.Resources.ProcessDocumentsWithMemoSubjectTemplateFormat(_obj.CommonSubjectPart);
        else
          e.Block.Subject = BusinessTripApprovalTasks.Resources.ProcessDocumentsSubjectTemplateFormat(_obj.CommonSubjectPart);
      }
      
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      businessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.OrderSigned;
    }
    
    #endregion
    
    #region Перечисление средств (блок 18)
    
    public virtual void StartBlock18(DirRX.BusinessTrips.Server.BusinessTripSimpleAssignmentArguments e)
    {
      var accountant = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.Accountant,
                                                                                                   Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(accountant);
      
      if (_obj.IsChangedByUser == true
          && _obj.GiveMoneyAssignment != null
          && _obj.GiveMoneyAssignment.Result == DirRX.BusinessTrips.BusinessTripSimpleAssignment.Result.Complete)
        
        e.Block.Subject = BusinessTripApprovalTasks.Resources.ChangeTransferMoneySubjectTemplateFormat(_obj.CommonSubjectPart);
      else
        e.Block.Subject = BusinessTripApprovalTasks.Resources.TransferMoneySubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void StartAssignment18(DirRX.BusinessTrips.IBusinessTripSimpleAssignment assignment, DirRX.BusinessTrips.Server.BusinessTripSimpleAssignmentArguments e)
    {
      _obj.GiveMoneyAssignment = assignment;
    }
    
    #endregion
    
    #region Деньги перечиcлены (блок 20)
    
    public virtual void StartBlock20(Sungero.Workflow.Server.NoticeArguments e)
    {
      // Уведомление о перечислении денег
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = BusinessTripApprovalTasks.Resources.TransferMoneyNoticeSubjectTemplateFormat(_obj.DepartureDate.Value.ToString("d"), _obj.ReturnDate.Value.ToString("d"), _obj.Purpose);
      
      _obj.MoneyTransferred = true;
    }
    
    #endregion
    
    #region Создание авансового отчета (блок 29)
    
    public virtual void Script29Execute()
    {
      var businessTrip = Functions.BusinessTripApprovalTask.GetBusinessTrip(_obj);
      
      // Создать авансовый отчет, если он ещё не был создан
      var document = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      
      if (document == null)
      {
        document = DirRX.ExpenseReports.PublicFunctions.Module.Remote.CreateExpenseReport(_obj.Employee, BusinessTripApprovalTasks.Resources.ExpenseReportPurpose, _obj.ExpensesSum.Value);
        
        _obj.ExpenseReportGroup.ExpenseReports.Add(document);

        businessTrip.ExpenseReport = document;
        businessTrip.Save();
      }
      else
        document.GettedMoney = _obj.ExpensesSum.Value;

      document.Save();

      // Связать авансовый отчет с приказами и служебной запиской. Требуется для выгрузки
      if (_obj.BusinessTripMemo != null)
        document.Relations.Add(DirRX.ExpenseReports.PublicConstants.Module.SimpleRelationName, _obj.BusinessTripMemo);
      
      var orders = BusinessTripOrders.GetAll(o => Equals(o.BusinessTrip, businessTrip));
      foreach(var order in orders)
      {
        document.Relations.Add(DirRX.ExpenseReports.PublicConstants.Module.SimpleRelationName, order);
      }
    }
    
    #endregion
    
  }
}