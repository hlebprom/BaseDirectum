using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTripMemo;

namespace DirRX.BusinessTrips
{
  partial class BusinessTripMemoClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      // Скрыть панель регистрации.      
      // Получить значение параметра, влияющего на отображение панели регистрации.
      bool showParamValue;
      var showParam = e.Params.TryGetValue(Sungero.Docflow.Constants.OfficialDocument.ShowParam, out showParamValue);
      // Выставить отрицательное значение параметра, чтобы при открытии формы скрывать панель регистрации.
      if (showParam)
        showParam = !showParamValue;
      // Сохранение отрицательного значения параметра.
      e.Params.AddOrUpdate(Sungero.Docflow.Constants.OfficialDocument.ShowParam, showParam);
    }

  }
}