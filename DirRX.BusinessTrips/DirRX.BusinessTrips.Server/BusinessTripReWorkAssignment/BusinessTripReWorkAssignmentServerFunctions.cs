﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripReWorkAssignment;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripReWorkAssignmentFunctions
  {

    /// <summary>
    /// Добавить суточные и проживание в расходы.
    /// </summary>
    [Remote, Public]
    public virtual void AddPerDiem()
    {
      
      // Добавить строку с суточными, если указаны даты и если даты начала и окончания командировки не совпадают.
      // Добавить с троку с проживанием, если указаны даты и если даты начала и окончания командировки не совпадают, и если даты прибытия и отъезда в городе не совпадают.
      if (_obj.ReturnDate.HasValue && _obj.DepartureDate.HasValue && _obj.ReturnDate != _obj.DepartureDate)
      {
        var perDiemExpenseType = DirRX.BusinessTrips.Functions.Module.GetPerDiemExpenseType();
        var hotelExpenseType = DirRX.BusinessTrips.Functions.Module.GetHotelExpenseType();
        
        var route = _obj.Route.OrderBy(r => r.DateIn);
        var routeDatesIn = _obj.Route.Select(r => r.DateIn).ToList();
        var finalRoutePointDateOut = _obj.Route.OrderByDescending(r => r.DateOut).FirstOrDefault().DateOut;
        var previousRoutePointFinalDay = _obj.DepartureDate;
        
        // Если в предыдущем городе были 1 день, днем начала пути в следующий город будет считаться этот же день.
        // Чтобы суточные начислялись по-человечески (1 день в городе1, 1 день в городе2, а не 1 день в городе1 и 2 дня в городе2), отслеживается, был ли предыдущий город городом однодневной поездки.
        var isPreviousRoutePointOneDay = false;
        
        foreach (var routePoint in route)
        {
          #region Проживание.
          if (routePoint.DateIn < routePoint.DateOut)
          {
            var hotelSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(hotelExpenseType, _obj.Employee, routePoint.Destination);
            if (hotelSetting != null)
            {
              var hotelExpense = _obj.Expenses.AddNew();
              hotelExpense.ExpenseType = hotelExpenseType;
              hotelExpense.LimitSetting = hotelSetting;
              
              if (hotelSetting.NoLimit != true)
              {
                var hotelLimit = hotelSetting.Limit.Value * (routePoint.DateOut.Value - routePoint.DateIn.Value).Duration().Days;
                hotelExpense.Limit = hotelLimit;
                hotelExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.HotelExpenseDescriptionTemplateFormat(routePoint.Destination.Name,
                                                                                                                                      routePoint.DateIn.Value.ToString("d"),
                                                                                                                                      routePoint.DateOut.Value.ToString("d"),
                                                                                                                                      hotelLimit.ToString("F", System.Globalization.CultureInfo.InvariantCulture));
              }
              else
                hotelExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.HotelNoLimitExpenseDescriptionTemplateFormat(routePoint.Destination.Name,
                                                                                                                                             routePoint.DateIn.Value.ToString("d"),
                                                                                                                                             routePoint.DateOut.Value.ToString("d"));
            }
          }
          #endregion
          
          #region Суточные.
          
          var duration = (routePoint.DateOut.Value - previousRoutePointFinalDay.Value).Duration().Days;
          
          // Увеличение количества дней на 1 для получения суточных в случаях, когда возвращаемся в город НОР или когда были в городе 1 день.
          if (routePoint.DateOut == _obj.ReturnDate && !isPreviousRoutePointOneDay || previousRoutePointFinalDay == routePoint.DateOut)
            duration++;
          
          // Если в предыдущем городе были 1 день, уменьшение количества дней на 1.
          if (isPreviousRoutePointOneDay && routePoint.DateIn != routePoint.DateOut && routePoint.DateOut != finalRoutePointDateOut)
            duration--;
          
          var perDiemSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(perDiemExpenseType, _obj.Employee, routePoint.Destination);
          if (perDiemSetting != null)
          {
            var perDiemString = string.Empty;
            
            var perDiemExpense = _obj.Expenses.AddNew();
            perDiemExpense.LimitSetting = perDiemSetting;
            perDiemExpense.ExpenseType = perDiemExpenseType;
            
            if (perDiemSetting.Limit.HasValue)
            {
              var perDiem = perDiemSetting.Limit.Value * duration;
              
              perDiemExpense.Limit = perDiem;
              perDiemExpense.ExpenseSum = perDiem;
              
              perDiemString = perDiemSetting.Limit.Value.ToString("F", System.Globalization.CultureInfo.InvariantCulture);
            }
            
            perDiemExpense.ExpenseDescription = DirRX.ExpenseReports.ExpenseReports.Resources.PerDiemExpenseDescriptionTemplateFormat(routePoint.Destination.Name,
                                                                                                                                      isPreviousRoutePointOneDay ?
                                                                                                                                      previousRoutePointFinalDay.Value.AddDays(1).ToString("d") :
                                                                                                                                      previousRoutePointFinalDay.Value.ToString("d"),
                                                                                                                                      routePoint.DateOut.Value.ToString("d"),
                                                                                                                                      duration.ToString(),
                                                                                                                                      perDiemString);
            
          }
          
          // Обновление значения по текущему городу.
          isPreviousRoutePointOneDay = routePoint.DateIn == routePoint.DateOut || (duration == 1 && isPreviousRoutePointOneDay);
          
          var dateToRemove = routeDatesIn.Where(d => d == routePoint.DateIn).FirstOrDefault();
          routeDatesIn.Remove(dateToRemove);
          
          if (!routeDatesIn.Where(d => d == routePoint.DateIn).Any())
            previousRoutePointFinalDay = routePoint.DateOut;
          
          #endregion
        }
        
        #region Суточные за обратную дорогу.
        
        // Добавить строку суточных за дорогу домой, если дата отъезда из последнего города в маршруте меньше даты окончания комнадировки.
        if (finalRoutePointDateOut < _obj.ReturnDate)
        {
          //Суточные за дорогу домой начисляются по городу расположения НОР. Если город не заполнен, будет подобрана Настройка лимита с пустым городом.
          var homeCity = Sungero.Commons.Cities.Null;
          if (_obj.Employee.Department.BusinessUnit != null)
            homeCity = _obj.Employee.Department.BusinessUnit.City;
          var homePerDiemSetting = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetLimitSetting(perDiemExpenseType, _obj.Employee, homeCity);
          if (homePerDiemSetting != null)
          {
            var homeRoadDuration = (_obj.ReturnDate.Value - finalRoutePointDateOut.Value).Duration().Days;
            
            if (!_obj.Route.Where(p => p.DateOut == _obj.DepartureDate).Any())
              homeRoadDuration++;
            
            var homePerDiemString = string.Empty;
            
            var homePerDiemExpense = _obj.Expenses.AddNew();
            homePerDiemExpense.LimitSetting = homePerDiemSetting;
            homePerDiemExpense.ExpenseType = perDiemExpenseType;
            
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
                                                                                                                                          _obj.ReturnDate.Value.ToString("d"),
                                                                                                                                          homeRoadDuration.ToString(),
                                                                                                                                          homePerDiemString);
          }
        }
        
        #endregion
        
      }
    }
    
  }
}