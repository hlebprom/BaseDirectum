using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.BusinessTrips.BusinessTripCancelTask;
using BusinessTripRoleGuids = DirRX.BusinessTrips.Constants.Module.BusinessTripRoleGuids;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripCancelTaskRouteHandlers
  {
    #region Удалить авансовый отчет
    
    public virtual void Script41Execute()
    {
      var expenseReport = _obj.BusinessTripApprovalTask.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      if (expenseReport != null)
      {
        _obj.BusinessTrip.ExpenseReport = null;
        _obj.BusinessTripApprovalTask.ExpenseReportGroup.ExpenseReports.Clear();
        // На случай ручных манипуляций удалить АО в try
        try
        {
          DirRX.ExpenseReports.ExpenseReports.Delete(expenseReport);
        }
        catch (Exception ex)
        {
          Logger.DebugFormat("Can not delete Expense report, id={0}. {1}", expenseReport.Id, ex.Message);
        }
      }
    }

    #endregion
    
    #region Удалиение неподписанного приказа об изменении
    
    public virtual void Script40Execute()
    {
      // Удалить последний неподписанный приказ об изменении
      // HACK .Last() не заработал, поэтому использую обратную сортировку по Id и .FirstOrDefault()
      var order = BusinessTripOrders.GetAll(o => Equals(o.BusinessTrip, _obj.BusinessTrip)).OrderByDescending(o => o.Id).FirstOrDefault();
      if (order != null && order.InternalApprovalState != DirRX.BusinessTrips.BusinessTripOrder.InternalApprovalState.Signed)
        DirRX.BusinessTrips.BusinessTripOrders.Delete(order);
    }
    
    #endregion

    #region Прекратить исходную задачу
    
    public virtual void Script39Execute()
    {
      _obj.BusinessTripApprovalTask.Abort();
    }
    
    #endregion

    #region Создание приказа об отмене
    
    public virtual void Script6Execute()
    {
      _obj.CancelOrderGroup.BusinessTripOrders.Add(PublicFunctions.Module.CreateCancelOrder(_obj.BusinessTrip, _obj.CancelReason));
    }
    
    #endregion

    #region Билеты куплены?
    
    public virtual bool Decision32Result()
    {
      return _obj.BusinessTripApprovalTask.TicketAndHotelProccessed.Value && _obj.BusinessTripApprovalTask.NeedTicketAndHotel.Value;
    }
    
    #endregion

    #region Возврат билетов
    
    public virtual void StartBlock33(DirRX.BusinessTrips.Server.TripCancelSimpleAssignmentArguments e)
    {
      var tiketsResponsible = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.TiketsResponsible,
                                                                                                          Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.BusinessTrip.Employee));
      e.Block.Performers.Add(tiketsResponsible);
      
      e.Block.Subject = BusinessTripCancelTasks.Resources.ReturnTiketsSubjectTemplateFormat(_obj.CommonSubjectPart);
    }

    #endregion
    
    #region Ознакомление с приказом
    
    public virtual void StartBlock31(DirRX.BusinessTrips.Server.TripCancelLearnOrderAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.BusinessTrip.Employee);
      
      e.Block.Subject = BusinessTripCancelTasks.Resources.LearnCancelOrderSubjectTemplateFormat(_obj.CommonSubjectPartNoFIO);
    }
    
    #endregion
    
    #region Удаление исходного приказа
    
    public virtual void Script14Execute()
    {
      // Удалить первоначальный приказ, если он есть
      if ( _obj.BusinessTripApprovalTask.OrderGroup.BusinessTripOrders.Any())
      {
        var order = _obj.BusinessTripApprovalTask.OrderGroup.BusinessTripOrders.FirstOrDefault();
        DirRX.BusinessTrips.BusinessTripOrders.Delete(order);
      }
    }

    #endregion
    
    #region Деньги выданы?
    
    public virtual bool Decision17Result()
    {
      return _obj.BusinessTripApprovalTask.MoneyTransferred.Value;
    }

    #endregion
    
    #region Уведомление об отмене командировки
    
    public virtual void StartBlock30(Sungero.Workflow.Server.NoticeArguments e)
    {
      // Получить пользователей для уведомления
      var recipientsForNotifications = _obj.BusinessTripApprovalTask.RecipientsForAbortNotification.Select(r => r.Recipient).Distinct().ToList();

      // Если деньги выданы, убрать бухгалтера из списка для уведомления
      if (_obj.BusinessTripApprovalTask.MoneyTransferred.Value)
      {
        var accountant = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.Accountant,
                                                                                                     Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.BusinessTrip.Employee));
        
        // Убрать бухгалтера из списка для уведомления
        recipientsForNotifications = recipientsForNotifications.Where(r => !r.Equals(accountant)).ToList();
      }
      
      // Убрать того, кто прекратил, из списка для уведомления
      recipientsForNotifications = recipientsForNotifications.Where(r => !r.Equals(_obj.CancelInitiator)).ToList();

      // Убрать подписанта приказа из списка для уведомления, если нужно подписывать новый приказ
      if (_obj.BusinessTripApprovalTask.OrderGroup.BusinessTripOrders.Any())
      {
        var orderSigner = Functions.Module.GetOrderSigner(Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.BusinessTrip.Employee));
        recipientsForNotifications = recipientsForNotifications.Where(r => !r.Equals(orderSigner)).ToList();
      }
      
      if (recipientsForNotifications.Any())
      {
        // Т.к. исполнителями блоков могут быть роли, составить список уникальных пользователей для отправки уведомлений
        IList<IUser> usersForNotifications = new List<IUser>();
        foreach(var recipient in recipientsForNotifications)
        {
          var user = Users.Null;
          if (Roles.Is(recipient))
          {
            var roleMember = Roles.As(recipient).RecipientLinks.FirstOrDefault();
            user = Users.As(roleMember.Member);
          }
          else
            user = Users.As(recipient);

          if (user != null && !usersForNotifications.Contains(user))
            usersForNotifications.Add(user);
        }
        
        foreach(var userForNotifications in usersForNotifications)
        {
          e.Block.Performers.Add(userForNotifications);
        }
      }
      
      e.Block.Subject = BusinessTripCancelTasks.Resources.CancelNotificationSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    #endregion
    
    #region Подписание приказа
    
    public virtual void StartBlock7(DirRX.BusinessTrips.Server.TripCancelSignOrderAssignmentArguments e)
    {
      var orderSigner = Functions.Module.GetOrderSigner(_obj.BusinessTrip.Employee.Department.BusinessUnit);
      if (orderSigner != null)
      {
        e.Block.Performers.Add(orderSigner);
        e.Block.Subject = BusinessTripCancelTasks.Resources.SignOrderSubjectTemplateFormat(_obj.CommonSubjectPart);
      }
    }
    
    #endregion
    
    #region Прием денег бухгалтером
    
    public virtual void StartBlock21(DirRX.BusinessTrips.Server.TripCancelSimpleAssignmentArguments e)
    {
      var accountant = DirRX.ExpenseReports.PublicFunctions.Module.GetRoleRecipientForBusinessUnit(BusinessTripRoleGuids.Accountant,
                                                                                                   Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.BusinessTrip.Employee));
      e.Block.Performers.Add(accountant);
      e.Block.Subject = BusinessTripCancelTasks.Resources.AccountantMoneyBackSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void EndBlock21(DirRX.BusinessTrips.Server.TripCancelSimpleAssignmentEndBlockEventArguments e)
    {
      if (_obj.MoneyReturnAssignment.Result == null)
        _obj.MoneyReturnAssignment.Complete(DirRX.BusinessTrips.TripCancelSimpleAssignment.Result.Complete);
    }
    
    #endregion
    
    #region Возврат денег сотрудником
    
    public virtual void StartBlock20(DirRX.BusinessTrips.Server.TripCancelSimpleAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.BusinessTrip.Employee);
      e.Block.Subject = BusinessTripCancelTasks.Resources.EmployeeMoneyBackSubjectTemplateFormat(_obj.BusinessTrip.DepartureDate.Value.ToString("d"),
                                                                                                 _obj.BusinessTrip.ReturnDate.Value.ToString("d"),
                                                                                                 _obj.BusinessTrip.Purpose);
    }
    
    public virtual void StartAssignment20(DirRX.BusinessTrips.ITripCancelSimpleAssignment assignment, DirRX.BusinessTrips.Server.TripCancelSimpleAssignmentArguments e)
    {
      _obj.MoneyReturnAssignment = assignment;
    }
    
    #endregion
    
    #region Изменение состояния командировки на "Отменена"
    
    public virtual void Script15Execute()
    {
      _obj.BusinessTrip.BusinessTripStatus = DirRX.BusinessTrips.BusinessTrip.BusinessTripStatus.Canceled;
      
      // Дописать причину отмены в примечание
      if (string.IsNullOrEmpty(_obj.BusinessTrip.Note))
        _obj.BusinessTrip.Note = BusinessTripCancelTasks.Resources.CancelReasonTemplateFormat(_obj.CancelReason);
      else
        _obj.BusinessTrip.Note = BusinessTripCancelTasks.Resources.CancelReasonForNoteTemplateFormat(_obj.BusinessTrip.Note.Trim(), _obj.CancelReason);
    }
    
    #endregion
    
    #region Исходный приказ подписан?
    
    public virtual bool Decision12Result()
    {
      if (_obj.BusinessTripApprovalTask.OrderGroup.BusinessTripOrders.Any())
      {
        var order = _obj.BusinessTripApprovalTask.OrderGroup.BusinessTripOrders.FirstOrDefault();
        return order.InternalApprovalState == DirRX.BusinessTrips.BusinessTripOrder.InternalApprovalState.Signed;
      }
      return false;
    }
    
    #endregion
    
    #region Удаление служебной записки
    
    public virtual void Script16Execute()
    {
      Functions.BusinessTripApprovalTask.DeleteMemo(_obj.BusinessTripApprovalTask);
    }
    
    #endregion
  }
}