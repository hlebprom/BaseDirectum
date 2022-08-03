using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripOrder;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripOrderSharedHandlers
  {

    public virtual void BusinessTripChanged(DirRX.BusinessTrips.Shared.BusinessTripOrderBusinessTripChangedEventArgs e)
    {
      if (e.NewValue != null)
      {
        _obj.BusinessUnit = e.NewValue.BusinessUnit;
        _obj.Department = e.NewValue.Department;

        _obj.Subject = e.NewValue.Name;
        
        // Сформировать строки для пунктов назначения.
        // В приказ нужно передавать первый и все остальные отдельно
        List<string> additionalDestinations = new List<string>();

        foreach (var tripLocation in e.NewValue.Route)
        {
          var orgName = String.Empty;
          if (!Equals(tripLocation.Org, Sungero.Parties.Companies.Null))
            orgName = tripLocation.Org.Name;
          
          additionalDestinations.Add(string.Format("{0} {1} {2}",
                                                   tripLocation.Destination.Country.Name,
                                                   tripLocation.Destination.Name,
                                                   orgName));
        }
        // Заполнить первый пункт назначения
        _obj.FirstDestinationPoint = additionalDestinations[0];
        // Удалить первый пункт назначения из списка
        additionalDestinations.RemoveAt(0);
        // Заполнить прочие пункты назначения
        if (additionalDestinations.Any())
          _obj.AdditionalDestinations = String.Join(System.Environment.NewLine, additionalDestinations);
        else
          _obj.AdditionalDestinations = " ";

        _obj.Duration = (int)e.NewValue.FactReturnDate.Value.Subtract(e.NewValue.FactDepartureDate.Value).TotalDays + 1;

        // Дата приказа = Сегодня
        // Если текущая дата больше даты отправления, то в качестве даты создания приказа использовать день, предшествующий дате отправления
        if (Calendar.Today > e.NewValue.DepartureDate)
          _obj.RegistrationDate = Calendar.AddWorkingDays(e.NewValue.DepartureDate.Value, -1);
        else
          _obj.RegistrationDate = Calendar.Today;
      }
    }

  }
}