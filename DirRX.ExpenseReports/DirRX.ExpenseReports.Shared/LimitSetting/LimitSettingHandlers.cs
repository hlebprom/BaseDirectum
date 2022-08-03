using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.LimitSetting;

namespace DirRX.ExpenseReports
{


  partial class LimitSettingSharedHandlers
  {

    public virtual void DepartmentChanged(DirRX.ExpenseReports.Shared.LimitSettingDepartmentChangedEventArgs e)
    {
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }

    public virtual void NoLimitChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue.Value)
        _obj.Limit = null;
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }

    public virtual void CitiesChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }

    public virtual void CountriesChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }

    public virtual void EmployeeChanged(DirRX.ExpenseReports.Shared.LimitSettingEmployeeChangedEventArgs e)
    {
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }

    public virtual void JobTitleChanged(DirRX.ExpenseReports.Shared.LimitSettingJobTitleChangedEventArgs e)
    {
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }
  }

}