using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.BusinessTrips.BusinessTripRecallTask;
using BusinessTripRoleGuids = DirRX.BusinessTrips.Constants.Module.BusinessTripRoleGuids;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripRecallTaskRouteHandlers
  {

    #region Указание даты отзыва в задаче по командировке
    
    public virtual void Script13Execute()
    {
      var businessTripApprovalTask = Functions.BusinessTrip.GetBusinessTripApprovalTask(_obj.BusinessTrip);
      businessTripApprovalTask.RecallDate = _obj.RecallDate;
    }

    #endregion
    
    #region Требуется переоформление билетов?
    
    public virtual bool Decision12Result()
    {
      var businessTripApprovalTask = Functions.BusinessTrip.GetBusinessTripApprovalTask(_obj.BusinessTrip);
      return businessTripApprovalTask.TicketAndHotelProccessed.Value && businessTripApprovalTask.NeedTicketAndHotel.Value;
    }
    
    #endregion

    #region Уведомление сотрудника
    
    public virtual void StartBlock11(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.BusinessTrip.Employee);
      
      e.Block.Subject = BusinessTripRecallTasks.Resources.RecallOrderNoticeSubjectTemplateFormat(_obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                 _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                 _obj.BusinessTrip.Purpose);
    }
    
    #endregion
    
    #region Ознакомление с приказом
    
    public virtual void StartBlock6(DirRX.BusinessTrips.Server.TripRecallLearnOrderAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.BusinessTrip.Employee);

      e.Block.Subject = BusinessTripRecallTasks.Resources.LearnRecallOrderSubjectTemplateFormat(_obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                _obj.BusinessTrip.Purpose);
    }

    #endregion
    
    #region Подписание приказа
    
    public virtual void StartBlock3(DirRX.BusinessTrips.Server.TripRecallSignOrderAssignmentArguments e)
    {
      var orderSigner = Functions.Module.GetOrderSigner(_obj.BusinessTrip.Employee.Department.BusinessUnit);
      if (orderSigner != null)
      {
        e.Block.Performers.Add(orderSigner);
        e.Block.Subject = BusinessTripRecallTasks.Resources.SignRecallOrderSubjectTemplateFormat(_obj.CommonSubjectPart);
      }
    }

    #endregion
    
    #region Переоформить билеты
    
    public virtual void StartBlock9(DirRX.BusinessTrips.Server.TripRecallSimpleAssignmentArguments e)
    {
      var tiketsResponsible = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.TiketsResponsible,
                                                                                                          Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.BusinessTrip.Employee));
      e.Block.Performers.Add(tiketsResponsible);
      
      e.Block.Subject = BusinessTripRecallTasks.Resources.ReBuyTiketsSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    #endregion

    #region Создание приказа об отзыве
    
    public virtual void Script4Execute()
    {
      var recallOrder = PublicFunctions.Module.CreateRecallOrder(_obj.BusinessTrip, _obj.RecallReason, _obj.RecallDate.Value);
      _obj.RecallOrderGroup.BusinessTripOrders.Add(recallOrder);
      
      var businessTripApprovalTask = Functions.BusinessTrip.GetBusinessTripApprovalTask(_obj.BusinessTrip);
      businessTripApprovalTask.OtherGroup.All.Add(recallOrder);
    }
    
    #endregion

  }
}