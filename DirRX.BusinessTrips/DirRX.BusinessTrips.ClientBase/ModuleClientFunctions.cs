using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using Sungero.Company;
using DocflowParamKeys = DirRX.BusinessTrips.Constants.Module.DocflowParamKeys;

namespace DirRX.BusinessTrips.Client
{
  public class ModuleFunctions
  {

    // HACK Вычисления действий для таблицы Расходы вынести в virtual функции, иначе из-за бага их не получется перекрыть.
    /// <summary>
    /// Рассчитать расходы для задачи по командировке.
    /// </summary>
    /// <param name="task">Задача.</param>
    public virtual void CalculateExpensesInTask(IBusinessTripApprovalTask task)
    {
      var expensesBeforeActionCount = task.Expenses.Count();
      
      var messages = new List<string> ();
      
      if (!task.DepartureDate.HasValue || !task.ReturnDate.HasValue)
        messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemErrorTextNoTripDates);
      if (!task.Route.Any())
        messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemErrorTextNoRoute);
      if (task.Route.Where(r => r.Destination == null || !r.DateIn.HasValue || !r.DateOut.HasValue).Any())
        messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemErrorTextNoRouteProperties);
      
      if (!messages.Any())
      {
        if (task.Route.Where(r => r.DateIn.Value < task.DepartureDate.Value).Any())
          messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripDepartureDateTooLateError);
        if (task.Route.Where(r => r.DateOut.Value > task.ReturnDate.Value).Any())
          messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripReturnDateTooEarlyError);
      }
      
      if (messages.Any())
        Dialogs.ShowMessage(string.Join(Environment.NewLine, messages), MessageType.Error);
      else if (task.DepartureDate == task.ReturnDate)
        Dialogs.ShowMessage(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemInfoNotForOneDayTrip, MessageType.Information);
      else
      {
        Functions.BusinessTripApprovalTask.AddPerDiem(task);
        
        var finalRoutePoint = task.Route.OrderByDescending(r => r.DateOut).FirstOrDefault();
        var addedExpensesCount = task.Expenses.Count() - expensesBeforeActionCount;
        var routePointsNoHotelCount = task.Route.Where(r => r.DateIn == r.DateOut).Count();
        var routePointsExpensesExpectedCount = (task.Route.Count() - routePointsNoHotelCount)*2;
        
        // Проверить, для всех ли городов заполнены 2 типа расходов - Проживание и Суточные.
        // В случае, если необходимы Суточные на дорогу из последнего города до дома, проверить, добавлена ли строка расходов для них.
        if (finalRoutePoint.DateOut == task.ReturnDate && (addedExpensesCount - routePointsNoHotelCount) < routePointsExpensesExpectedCount
            || finalRoutePoint.DateOut < task.ReturnDate && (addedExpensesCount - 1 - routePointsNoHotelCount) < routePointsExpensesExpectedCount)
        {
          Dialogs.ShowMessage(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemInfoNoSettings, MessageType.Information);
        }
      }
    }

    // HACK Вычисления действий для таблицы Расходы вынести в virtual функции, иначе из-за бага их не получется перекрыть.
    /// <summary>
    /// Рассчитать расходы для задания на доработку по командировке.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public virtual void CalculateExpensesInReWorkAssignment(IBusinessTripReWorkAssignment assignment)
    {
      var expensesBeforeActionCount = assignment.Expenses.Count();
      
      var messages = new List<string> ();
      
      if (!assignment.DepartureDate.HasValue || !assignment.ReturnDate.HasValue)
        messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemErrorTextNoTripDates);
      if (!assignment.Route.Any())
        messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemErrorTextNoRoute);
      if (assignment.Route.Where(r => r.Destination == null || !r.DateIn.HasValue || !r.DateOut.HasValue).Any())
        messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemErrorTextNoRouteProperties);
      
      if (!messages.Any())
      {
        if (assignment.Route.Where(r => r.DateIn.Value < assignment.DepartureDate.Value).Any())
          messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripDepartureDateTooLateError);
        if (assignment.Route.Where(r => r.DateOut.Value > assignment.ReturnDate.Value).Any())
          messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripReturnDateTooEarlyError);
      }
      
      if (messages.Any())
        Dialogs.ShowMessage(string.Join(Environment.NewLine, messages), MessageType.Error);
      else if (assignment.DepartureDate == assignment.ReturnDate)
        Dialogs.ShowMessage(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemInfoNotForOneDayTrip, MessageType.Information);
      else
      {
        Functions.BusinessTripReWorkAssignment.AddPerDiem(assignment);
        
        var finalRoutePoint = assignment.Route.OrderByDescending(r => r.DateOut).FirstOrDefault();
        var addedExpensesCount = assignment.Expenses.Count() - expensesBeforeActionCount;
        var routePointsNoHotelCount = assignment.Route.Where(r => r.DateIn == r.DateOut).Count();
        var routePointsExpensesExpectedCount = (assignment.Route.Count() - routePointsNoHotelCount)*2;
        
        // Проверить, для всех ли городов заполнены 2 типа расходов - Проживание и Суточные.
        // В случае, если необходимы Суточные на дорогу из последнего города до дома, проверить, добавлена ли строка расходов для них.
        if (finalRoutePoint.DateOut == assignment.ReturnDate && (addedExpensesCount - routePointsNoHotelCount) < routePointsExpensesExpectedCount
            || finalRoutePoint.DateOut < assignment.ReturnDate && (addedExpensesCount - 1 - routePointsNoHotelCount) < routePointsExpensesExpectedCount)
        {
          Dialogs.ShowMessage(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemInfoNoSettings, MessageType.Information);
        }
      }
    }
    
    // HACK Вычисления действий для таблицы Расходы вынести в virtual функции, иначе из-за бага их не получется перекрыть.
    /// <summary>
    /// Рассчитать расходы для задания на подготовку авансового отчета по командировке.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public virtual void CalculateExpensesInPrepareReportAssignment(IBusinessTripPrepareReportAssignment assignment)
    {
      var expensesBeforeActionCount = assignment.Expenses.Count();
      
      var messages = new List<string> ();
      
      if (!assignment.FactDepartureDate.HasValue || !assignment.FactReturnDate.HasValue)
        messages.Add(DirRX.BusinessTrips.BusinessTripPrepareReportAssignments.Resources.AddPerDiemErrorTextNoFactTripDates);
      if (assignment.Route.Where(r => !r.FactDateIn.HasValue || !r.FactDateOut.HasValue).Any())
        messages.Add(DirRX.BusinessTrips.BusinessTripPrepareReportAssignments.Resources.AddPerDiemErrorTextNoRouteFactDates);
      
      if (!messages.Any())
      {
        if (assignment.Route.Where(r => r.FactDateIn.Value < assignment.FactDepartureDate.Value).Any())
          messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripDepartureDateTooLateError);
        if (assignment.Route.Where(r => r.FactDateOut.Value > assignment.FactReturnDate.Value).Any())
          messages.Add(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.TripReturnDateTooEarlyError);
      }
      
      if (messages.Any())
        Dialogs.ShowMessage(string.Join(Environment.NewLine, messages), MessageType.Error);
      else if (assignment.FactDepartureDate == assignment.FactReturnDate)
        Dialogs.ShowMessage(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemInfoNotForOneDayTrip, MessageType.Information);
      else
      {
        Functions.BusinessTripPrepareReportAssignment.AddPerDiem(assignment);
        
        var finalRoutePoint = assignment.Route.OrderByDescending(r => r.FactDateOut).FirstOrDefault();
        var addedExpensesCount = assignment.Expenses.Count() - expensesBeforeActionCount;
        var routePointsNoHotelCount = assignment.Route.Where(r => r.FactDateIn == r.FactDateOut).Count();
        var routePointsExpensesExpectedCount = (assignment.Route.Count() - routePointsNoHotelCount)*2;
        
        // Проверить, для всех ли городов заполнены 2 типа расходов - Проживание и Суточные.
        // В случае, если необходимы Суточные на дорогу из последнего города до дома, проверить, добавлена ли строка расходов для них.
        if (finalRoutePoint.FactDateOut == assignment.FactReturnDate && (addedExpensesCount - routePointsNoHotelCount) < routePointsExpensesExpectedCount
            || finalRoutePoint.FactDateOut < assignment.FactReturnDate && (addedExpensesCount - 1 - routePointsNoHotelCount) < routePointsExpensesExpectedCount)
        {
          Dialogs.ShowMessage(DirRX.BusinessTrips.BusinessTripApprovalTasks.Resources.AddPerDiemInfoNoSettings, MessageType.Information);
        }
      }
    }
    
    /// <summary>
    /// Выбор значений настроек модуля.
    /// </summary>
    [Public]
    public virtual void SelectModuleSettings()
    {
      // Менять настройки может только администратор
      if (Users.Current.IncludedIn(Roles.Administrators))
      {
        var dialog = Dialogs.CreateInputDialog(Resources.ModuleSettings);
        
        // Суточные.
        var perDiemExpenseTypeId = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(DocflowParamKeys.PerDiemExpenseTypeIdKey);
        DirRX.ExpenseReports.IExpenseType perDiemExpenseTypeForSelect = null;
        if (!string.IsNullOrEmpty(perDiemExpenseTypeId))
          perDiemExpenseTypeForSelect = DirRX.ExpenseReports.PublicFunctions.ExpenseType.Remote.GetExpenseTypeById(Convert.ToInt32(perDiemExpenseTypeId));
        
        // Проживание.
        var hotelExpenseTypeId = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(DocflowParamKeys.HotelExpenseTypeIdKey);
        DirRX.ExpenseReports.IExpenseType hotelExpenseTypeForSelect = null;
        if (!string.IsNullOrEmpty(hotelExpenseTypeId))
          hotelExpenseTypeForSelect = DirRX.ExpenseReports.PublicFunctions.ExpenseType.Remote.GetExpenseTypeById(Convert.ToInt32(hotelExpenseTypeId));
        
        var dialogPerDiemExpenseTypeSelect = dialog.AddSelect(Resources.PerDiemExpenseTypeSelect, true, perDiemExpenseTypeForSelect);
        var dialogHotelExpenseTypeSelect = dialog.AddSelect(Resources.HotelExpenseTypeSelect, true, hotelExpenseTypeForSelect);
        
        // Отфильтровать только действующие типы расходов
        var activeExpenseTypes = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetActiveExpenseTypes();
        dialogPerDiemExpenseTypeSelect.From(activeExpenseTypes);
        dialogHotelExpenseTypeSelect.From(activeExpenseTypes);
        
        if (dialog.Show() == DialogButtons.Ok)
        {
          // Суточные.
          if (!Equals(dialogPerDiemExpenseTypeSelect.Value, perDiemExpenseTypeForSelect))
            DirRX.ExpenseReports.PublicFunctions.Module.Remote.AddDocflowParam(DocflowParamKeys.PerDiemExpenseTypeIdKey, dialogPerDiemExpenseTypeSelect.Value.Id.ToString());
          
          // Проживание.
          if (!Equals(dialogHotelExpenseTypeSelect.Value, hotelExpenseTypeForSelect))
            DirRX.ExpenseReports.PublicFunctions.Module.Remote.AddDocflowParam(DocflowParamKeys.HotelExpenseTypeIdKey, dialogHotelExpenseTypeSelect.Value.Id.ToString());
        }
      }
      else
        Dialogs.ShowMessage(DirRX.BusinessTrips.Resources.ModuleSettingsAccessErrorText, MessageType.Warning);
    }

    /// <summary>
    /// Показать все мои командировки.
    /// </summary>
    [Public]
    public virtual void ShowMyBusinessTrips()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.GetMyBusinessTrips().Show();
      else
        Dialogs.ShowMessage(DirRX.BusinessTrips.Resources.CantEmplementByNotEmployee);
    }

    /// <summary>
    /// Показать все мои прошедшие командировки.
    /// </summary>
    [Public]
    public virtual void ShowMyPastBusinessTrips()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.GetMyPastBusinessTrips().Show();
      else
        Dialogs.ShowMessage(DirRX.BusinessTrips.Resources.CantEmplementByNotEmployee);
    }

    /// <summary>
    /// Показать все мои предстоящие командировки.
    /// </summary>
    [Public]
    public virtual void ShowMyFutureBusinessTrips()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.GetMyFutureBusinessTrips().Show();
      else
        Dialogs.ShowMessage(DirRX.BusinessTrips.Resources.CantEmplementByNotEmployee);
    }
    
    /// <summary>
    /// Создать задачу на согласование командировки.
    /// </summary>
    [Public]
    public virtual void CreateNewBusinessTripApprovalTask()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.CreateNewBusinessTripApprovalTask().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
  }
}