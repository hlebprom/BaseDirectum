using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.BusinessTrips.BusinessTripChangeOrderTask;
using BusinessTripRoleGuids = DirRX.BusinessTrips.Constants.Module.BusinessTripRoleGuids;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripChangeOrderTaskRouteHandlers
  {

    #region Создание приказа об изменении
    
    public virtual void Script3Execute()
    {
      _obj.ChangeOrderGroup.BusinessTripOrders.Add(PublicFunctions.Module.CreateChangeOrder(_obj.BusinessTrip, _obj.ChangeReason));
    }

    #endregion
    
    #region Подписание приказа
    
    public virtual void StartBlock4(DirRX.BusinessTrips.Server.ChangeOrderSignAssignmentArguments e)
    {
      var orderSigner = Functions.Module.GetOrderSigner(_obj.BusinessTrip.Employee.Department.BusinessUnit);
      
      if (orderSigner != null)
      {
        e.Block.Performers.Add(orderSigner);
        e.Block.Subject = BusinessTripChangeOrderTasks.Resources.SignOrderSubjectTemplateFormat(_obj.BusinessTrip.Employee.Name,
                                                                                                _obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                _obj.BusinessTrip.Purpose);
      }
    }

    #endregion
    
    #region Ознакомление с приказом
    
    public virtual void StartBlock5(DirRX.BusinessTrips.Server.ChangeOrderLearnAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.BusinessTrip.Employee);
      
      e.Block.Subject = BusinessTripChangeOrderTasks.Resources.LearnChangeOrderSubjectTemplateFormat(_obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                     _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                     _obj.BusinessTrip.Purpose);
    }

    #endregion
    
  }
}