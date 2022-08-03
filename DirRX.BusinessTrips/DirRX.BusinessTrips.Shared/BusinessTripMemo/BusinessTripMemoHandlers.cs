using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripMemo;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripMemoSharedHandlers
  {

    public virtual void BusinessTripChanged(DirRX.BusinessTrips.Shared.BusinessTripMemoBusinessTripChangedEventArgs e)
    {
      if (e.NewValue != null)
      {
        _obj.Subject = BusinessTripMemos.Resources.SubjectTemplateFormat(e.NewValue.Employee.Name, 
                                                                         e.NewValue.DepartureDate.Value.ToString("d"), 
                                                                         e.NewValue.ReturnDate.Value.ToString("d"), 
                                                                         e.NewValue.Purpose);
        
        // Определить пункт отправления сотрудника
        var businessUnit = e.NewValue.Employee.Department.BusinessUnit;
        var cityDeparture = Sungero.Commons.Cities.Null;
        if (businessUnit != null)
          cityDeparture = businessUnit.City;
        
        // В качестве пункта отправления подставляем "из <Пункт отправления>", либо пробел, чтобы в шаблоне не отображалось стандартное значение поля
        _obj.CityDeparture = (cityDeparture != null) ? BusinessTripMemos.Resources.CityDepartureTemlateFormat(cityDeparture.Name) : " ";
        
        _obj.CityArrival = e.NewValue.Route.Last().Destination.Name;
        _obj.Duration = (int)e.NewValue.ReturnDate.Value.Subtract(e.NewValue.DepartureDate.Value).TotalDays + 1;
        
        _obj.BusinessUnit = e.NewValue.BusinessUnit;
        _obj.Department = e.NewValue.Department;
      }
    }
  }

}