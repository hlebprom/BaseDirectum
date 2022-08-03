using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.LimitSetting;

namespace DirRX.ExpenseReports.Shared
{
  partial class LimitSettingFunctions
  {

    /// <summary>
    /// Установить состояние свойств.
    /// </summary>
    public void SetStatePropertiesSettings()
    {      
      _obj.State.Properties.Limit.IsEnabled = _obj.NoLimit != true;
      _obj.State.Properties.Limit.IsRequired = _obj.NoLimit != true;
      
      _obj.State.Properties.Department.IsRequired = _obj.JobTitle != null;
      
      _obj.State.Properties.Countries.IsEnabled = !_obj.Cities.Any();
      _obj.State.Properties.Cities.IsEnabled = !_obj.Countries.Any();
      
      _obj.State.Properties.Department.IsEnabled = _obj.Employee == null;
      _obj.State.Properties.JobTitle.IsEnabled = _obj.Employee == null;
      
      _obj.State.Properties.Employee.IsEnabled = _obj.Department == null && _obj.JobTitle == null;
    }

  }
}