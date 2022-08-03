using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.LimitSetting;

namespace DirRX.ExpenseReports.Server
{
  partial class LimitSettingFunctions
  {

    /// <summary>
    /// Построить модель состояния с инструкцией.
    /// </summary>
    /// <returns>Модель состояния.</returns>    
    [Remote]
    public StateView GetInstructionState()
    {
      return Functions.Module.GetTextState(DirRX.ExpenseReports.LimitSettings.Resources.PriorityInstruction);
    }

    /// <summary>
    /// Получить дублирующие записи.
    /// </summary>
    /// <returns>Список дублирующих записей.</returns>
    [Remote(IsPure = true)]
    public virtual List<ILimitSetting> GetDuplicates()
    {
      var limitSettings = DirRX.ExpenseReports.LimitSettings.GetAll(s => !Equals(_obj, s)
                                                                    && Equals(s.ExpenseType, _obj.ExpenseType)
                                                                    && Equals(s.Department, _obj.Department)
                                                                    && Equals(s.Employee, _obj.Employee)
                                                                    && s.Status == DirRX.ExpenseReports.LimitSetting.Status.Active).ToList();
      
      //HACK: В linq падает ошибка с Intersect (не поддерживается), приходится делать отдельно.
      
      // Для представления результата необходим отдельный список, чтобы в процессе обработки не менялся limitSettings.
      var result = new List<ILimitSetting> ();
      
      if (limitSettings.Any())
      {
        // Для стран и городов каждого элемента списка limitSettings проверить пересекаемость со странами и городами в _obj.
        foreach (var setting in limitSettings)
        {
          if (setting.Countries.Select(c => c.Country).Intersect(_obj.Countries.Select(c => c.Country)).Any() 
              || setting.Cities.Select(c => c.City).Intersect(_obj.Cities.Select(c => c.City)).Any())
            result.Add(setting);
        }
      }
      
      return result;
    }

  }
}