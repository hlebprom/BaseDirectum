using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripPrepareReportAssignment;
using System.Globalization;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripPrepareReportAssignmentFunctions
  {
    /// <summary>
    /// Построить модель состояния с инструкцией.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public virtual StateView GetPrepareExpenseReportAssignmentState()
    {
      return DirRX.ExpenseReports.PublicFunctions.Module.GetTextState(
        BusinessTripPrepareReportAssignments.Resources.HowToFillExpenseReportInstructionFormat(
          DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(
            DirRX.ExpenseReports.PublicConstants.Module.DocflowParamKeys.SupportingDocsMailKey)));
    }

    /// <summary>
    /// Добавить суточные и проживание в расходы.
    /// </summary>
    [Remote, Public]
    public virtual void AddPerDiem()
    {
      // Добавить строку с суточными, если указаны даты и если даты начала и окончания командировки не совпадают.
      // Добавить с троку с проживанием, если указаны даты и если даты начала и окончания командировки не совпадают, и если даты прибытия и отъезда в городе не совпадают.
      if (_obj.FactReturnDate.HasValue && _obj.FactDepartureDate.HasValue && _obj.FactReturnDate != _obj.FactDepartureDate)
      {
        var perDiemExpenseType = DirRX.BusinessTrips.Functions.Module.GetPerDiemExpenseType();
        var hotelExpenseType = DirRX.BusinessTrips.Functions.Module.GetHotelExpenseType();
        
        var route = _obj.Route.OrderBy(r => r.FactDateIn);
        var routeDatesIn = _obj.Route.Select(r => r.FactDateIn).ToList();
        var finalRoutePointDateOut = _obj.Route.OrderByDescending(r => r.FactDateOut).FirstOrDefault().FactDateOut;
        var previousRoutePointFinalDay = _obj.FactDepartureDate;
        
        var employee = DirRX.BusinessTrips.BusinessTripApprovalTasks.As(_obj.Task).Employee;
        
        // Если в предыдущем городе были 1 день, днем начала пути в следующий город будет считаться этот же день.
        // Чтобы суточные начислялись по-человечески (1 день в городе1, 1 день в городе2, а не 1 день в городе1 и 2 дня в городе2), отслеживается, был ли предыдущий город городом однодневной поездки.
        var isPreviousRoutePointOneDay = false;
        
        foreach (var routePoint in route)
        {
          #region Проживание.
          if (routePoint.FactDateIn < routePoint.FactDateOut)
          {
            var hotelSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(hotelExpenseType, employee, routePoint.Destination);
            if (hotelSetting != null)
            {
              var hotelExpense = _obj.Expenses.AddNew();
              hotelExpense.ExpenseType = hotelExpenseType;
              hotelExpense.LimitSetting = hotelSetting;
              hotelExpense.ExpenseDate = routePoint.FactDateIn;
              
              if (hotelSetting.NoLimit != true)
              {
                var hotelLimit = hotelSetting.Limit.Value * (routePoint.FactDateOut.Value - routePoint.FactDateIn.Value).Duration().Days;
                hotelExpense.Limit = hotelLimit;
                hotelExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.HotelExpenseDescriptionTemplateFormat(routePoint.Destination.Name,
                                                                                                                                      routePoint.FactDateIn.Value.ToString("d"),
                                                                                                                                      routePoint.FactDateOut.Value.ToString("d"),
                                                                                                                                      hotelLimit.ToString("F", System.Globalization.CultureInfo.InvariantCulture));
              }
              else
                hotelExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.HotelNoLimitExpenseDescriptionTemplateFormat(routePoint.Destination.Name,
                                                                                                                                             routePoint.FactDateIn.Value.ToString("d"),
                                                                                                                                             routePoint.FactDateOut.Value.ToString("d"));
            }
          }
          #endregion
          
          #region Суточные.
          
          var duration = (routePoint.FactDateOut.Value - previousRoutePointFinalDay.Value).Duration().Days;
          
          // Увеличение количества дней на 1 для получения суточных в случаях, когда возвращаемся в город НОР или когда были в городе 1 день.
          if (routePoint.FactDateOut == _obj.FactReturnDate && !isPreviousRoutePointOneDay || previousRoutePointFinalDay == routePoint.FactDateOut)
            duration++;
          
          // Если в предыдущем городе были 1 день, уменьшение количества дней на 1.
          if (isPreviousRoutePointOneDay && routePoint.FactDateIn != routePoint.FactDateOut && routePoint.FactDateOut != finalRoutePointDateOut)
            duration--;
          
          var perDiemSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(perDiemExpenseType, employee, routePoint.Destination);
          if (perDiemSetting != null)
          {
            var perDiemString = string.Empty;
            
            var routePointIn = previousRoutePointFinalDay;
            if (isPreviousRoutePointOneDay)
              routePointIn = previousRoutePointFinalDay.Value.AddDays(1);
            
            var perDiemExpense = _obj.Expenses.AddNew();
            perDiemExpense.LimitSetting = perDiemSetting;
            perDiemExpense.ExpenseType = perDiemExpenseType;
            perDiemExpense.ExpenseDate = routePointIn;
            
            if (perDiemSetting.Limit.HasValue)
            {
              var perDiem = perDiemSetting.Limit.Value * duration;
              
              perDiemExpense.Limit = perDiem;
              perDiemExpense.ExpenseSum = perDiem;
              
              perDiemString = perDiemSetting.Limit.Value.ToString("F", System.Globalization.CultureInfo.InvariantCulture);
            }
            
            perDiemExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.PerDiemExpenseDescriptionTemplateFormat(routePoint.Destination.Name,
                                                                                                                                      routePointIn.Value.ToString("d"),
                                                                                                                                      routePoint.FactDateOut.Value.ToString("d"),
                                                                                                                                      duration.ToString(),
                                                                                                                                      perDiemString);
            
          }
          
          // Обновление значения по текущему городу.
          isPreviousRoutePointOneDay = routePoint.FactDateIn == routePoint.FactDateOut || (duration == 1 && isPreviousRoutePointOneDay);
          
          var dateToRemove = routeDatesIn.Where(d => d == routePoint.FactDateIn).FirstOrDefault();
          routeDatesIn.Remove(dateToRemove);
          
          if (!routeDatesIn.Where(d => d == routePoint.FactDateIn).Any())
            previousRoutePointFinalDay = routePoint.FactDateOut;
          #endregion
        }
        
        #region Суточные за обратную дорогу.
        
        // Добавить строку суточных за дорогу домой, если дата отъезда из последнего города в маршруте меньше даты окончания комнадировки.
        if (finalRoutePointDateOut < _obj.FactReturnDate)
        {
          //Суточные за дорогу домой начисляются по городу расположения НОР. Если город не заполнен, будет подобрана Настройка лимита с пустым городом.
          var homeCity = Sungero.Commons.Cities.Null;
          if (employee.Department.BusinessUnit != null)
            homeCity = employee.Department.BusinessUnit.City;
          var homePerDiemSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(perDiemExpenseType, employee, homeCity);
          if (homePerDiemSetting != null)
          {
            var homeRoadDuration = (_obj.FactReturnDate.Value - finalRoutePointDateOut.Value).Duration().Days;
            
            if (!_obj.Route.Where(p => p.FactDateOut == _obj.FactDepartureDate).Any())
              homeRoadDuration++;
            
            var homePerDiemString = string.Empty;
            
            var homePerDiemExpense = _obj.Expenses.AddNew();
            homePerDiemExpense.LimitSetting = homePerDiemSetting;
            homePerDiemExpense.ExpenseType = perDiemExpenseType;
            homePerDiemExpense.ExpenseDate = finalRoutePointDateOut;
            
            if (homePerDiemSetting.Limit.HasValue)
            {
              var homePerDiem = homePerDiemSetting.Limit.Value * homeRoadDuration;
              homePerDiemExpense.Limit = homePerDiem;
              homePerDiemExpense.ExpenseSum = homePerDiem;
              
              homePerDiemString = homePerDiemSetting.Limit.Value.ToString("F", System.Globalization.CultureInfo.InvariantCulture);
            }
            
            var returnCityStr = DirRX.ExpenseReports.ExpenseReports.Resources.PerDiemExpenseDescriptionTemplateBusinessUnit;
            
            homePerDiemExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.PerDiemExpenseDescriptionTemplateFormat(homeCity != null ? homeCity.Name : returnCityStr,
                                                                                                                                          finalRoutePointDateOut.Value.ToString("d"),
                                                                                                                                          _obj.FactReturnDate.Value.ToString("d"),
                                                                                                                                          homeRoadDuration.ToString(),
                                                                                                                                          homePerDiemString);
          }
        }
        
        #endregion
        
      }
    }
    
    /// <summary>
    /// Скопировать таблицу расходов из авансового отчета.
    /// </summary>
    [Remote, Public]
    public virtual void GetExpensesFromReport()
    {
      var expenseReport = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      foreach(var expense in expenseReport.Expenses)
      {
        var jobExpense = _obj.Expenses.AddNew();
        jobExpense.SupportingDoc = expense.SupportingDoc;
        jobExpense.ExpenseDescription = expense.ExpenseDescription;
        jobExpense.ExpenseDate = expense.ExpenseDate;
        jobExpense.ExpenseSumInCurrency = expense.ExpenseSumInCurrency;
        jobExpense.Currency = expense.Currency;
        jobExpense.ExpenseSum = expense.ExpenseSum;
        jobExpense.ExpenseNumber = expense.ExpenseNumber;
      }
    }
    
  }
}