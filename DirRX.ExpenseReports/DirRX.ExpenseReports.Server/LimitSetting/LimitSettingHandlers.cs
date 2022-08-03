using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.LimitSetting;

namespace DirRX.ExpenseReports
{


  partial class LimitSettingCitiesCityPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CitiesCityFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_root.Countries.Any())
      {
        var countries = _root.Countries.Select(c => c.Country);
        return query.Where(q => countries.Contains(q.Country));
      }
      return query;
    }
  }

  partial class LimitSettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.NoLimit = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {      
      if (Functions.LimitSetting.GetDuplicates(_obj).Any())
        e.AddError(LimitSettings.Resources.DuplicateDetected, _obj.Info.Actions.ShowDuplicates);
      
      var nameDetalization = string.Empty;
      
      if (_obj.Employee != null)
        nameDetalization = LimitSettings.Resources.NameTemplateEmployeeFormat(_obj.Employee.Name);
      else if (_obj.JobTitle != null)
        nameDetalization = LimitSettings.Resources.NameTemplateJobTitleDepartmentFormat(_obj.JobTitle.Name, _obj.Department.Name);
      else if (_obj.Department != null)
        nameDetalization = LimitSettings.Resources.NameTemplateDepartmentFormat(_obj.Department.Name);
      
      _obj.Name = LimitSettings.Resources.NameTemplateFormat(_obj.Limit.HasValue ? _obj.Limit.Value.ToString() : LimitSettings.Resources.NameTemplateNoLimit, 
                                                             _obj.ExpenseType.Name, 
                                                             nameDetalization);
    }
  }

}