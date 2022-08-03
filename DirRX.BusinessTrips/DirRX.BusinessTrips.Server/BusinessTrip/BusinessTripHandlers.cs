using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTrip;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      // HACK Заполнить источник финансирования пробелом, чтобы в шаблон корректно подставлялось пустое значение
      _obj.FinanceSource = " ";
      
      _obj.BusinessTripStatus = BusinessTripStatus.Started;
    }
  }

  partial class BusinessTripFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter == null)
        return query;
      
      var endDate = Calendar.UserToday;
      var beginDate = Calendar.UserToday.AddDays(-7);
      
      if (_filter.CurrentWeek)
        beginDate = Calendar.UserToday.AddDays(-7);
      
      if (_filter.CurrentMonth)
      	beginDate = Calendar.UserToday.AddMonths(-1);
      
      if (_filter.CurrentQuarter)
      	beginDate = Calendar.UserToday.AddMonths(-3);
      
      if (_filter.CurrentYear)
      	beginDate = Calendar.UserToday.AddYears(-1);
      
      if (_filter.ShowPeriod)
      {
      	beginDate = _filter.DateRangeFrom ?? Calendar.SqlMinValue;
        endDate = _filter.DateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      query = query.Where(t => t.DepartureDate.Between(beginDate, endDate)
                          || t.ReturnDate.Between(beginDate, endDate)
                          || (t.DepartureDate < beginDate && t.ReturnDate > endDate));
      
      return query;
    }
  }

}