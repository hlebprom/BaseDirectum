using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.ExpenseReports.LimitSetting;

namespace DirRX.ExpenseReports
{
  partial class LimitSettingClientHandlers
  {

    public virtual void LimitValueInput(Sungero.Presentation.DoubleValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue.Value <= 0)
        e.AddError(LimitSettings.Resources.SubzeroError);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.LimitSetting.SetStatePropertiesSettings(_obj);
    }
  }

}