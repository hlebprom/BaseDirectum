using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.ExpenseReports.ExpenseRequestApprovalTask;

namespace DirRX.ExpenseReports.Server
{
  partial class ExpenseRequestApprovalTaskRouteHandlers
  {

    #region Согласование руководителем (блок 3)
    
    public virtual void StartBlock3(DirRX.ExpenseReports.Server.ExpenseRequestBossApprovalAssignmentArguments e)
    {
      var manager = Functions.Module.GetManager(_obj.Employee);
      if (manager != null)
      {
        e.Block.Performers.Add(manager);
        e.Block.Subject = ExpenseRequestApprovalTasks.Resources.BossApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
      }
    }
    
    public virtual void StartAssignment3(DirRX.ExpenseReports.IExpenseRequestBossApprovalAssignment assignment, DirRX.ExpenseReports.Server.ExpenseRequestBossApprovalAssignmentArguments e)
    {
      // Скопировать значения свойств из задачи
      assignment.Purpose = _obj.Purpose;
      assignment.ExpenseSum = _obj.ExpenseSum;
      assignment.ExpenseReportDate = _obj.ExpenseReportDate;
      
      assignment.Expenses.Clear();
      
      foreach(var expense in _obj.Expenses)
      {
        var assignmentExpense = assignment.Expenses.AddNew();
        assignmentExpense.ExpenseType = expense.ExpenseType;
        assignmentExpense.ExpenseSum = expense.ExpenseSum;
        assignmentExpense.ExpenseDescription = expense.ExpenseDescription;
        assignmentExpense.LimitSetting = expense.LimitSetting;
      }
    }
    
    
    #endregion
    
    #region Согласование бухгалтером (блок 5)
    
    public virtual void StartBlock5(DirRX.ExpenseReports.Server.ExpenseRequestAccountantApprovalAssignmentArguments e)
    {
      var accountant = Functions.Module.GetRoleRecipientForBusinessUnit(DirRX.ExpenseReports.Constants.Module.ExpenseReportRoleGuids.Accountant,
                                                                        Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(accountant);
      e.Block.Subject = ExpenseRequestApprovalTasks.Resources.TransferMoneyAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void StartAssignment5(DirRX.ExpenseReports.IExpenseRequestAccountantApprovalAssignment assignment, DirRX.ExpenseReports.Server.ExpenseRequestAccountantApprovalAssignmentArguments e)
    {
      // Скопировать значения свойств из задачи
      assignment.Purpose = _obj.Purpose;
      assignment.ExpenseSum = _obj.ExpenseSum;
      assignment.ExpenseReportDate = _obj.ExpenseReportDate;
      
      assignment.Expenses.Clear();
      
      foreach(var expense in _obj.Expenses)
      {
        var assignmentExpense = assignment.Expenses.AddNew();
        assignmentExpense.ExpenseType = expense.ExpenseType;
        assignmentExpense.ExpenseSum = expense.ExpenseSum;
        assignmentExpense.ExpenseDescription = expense.ExpenseDescription;
        assignmentExpense.LimitSetting = expense.LimitSetting;
      }
    }
    
    #endregion
    
    #region Доработка (блок 6)
    
    public virtual void StartBlock6(DirRX.ExpenseReports.Server.ExpenseRequestReWorkAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseRequestApprovalTasks.Resources.ReWorkAssignmentSubjectTemplateFormat(_obj.ExpenseSum, _obj.Purpose);
    }
    
    public virtual void StartAssignment6(DirRX.ExpenseReports.IExpenseRequestReWorkAssignment assignment, DirRX.ExpenseReports.Server.ExpenseRequestReWorkAssignmentArguments e)
    {
      // Скопировать значения свойств из задачи
      assignment.Purpose = _obj.Purpose;
      assignment.ExpenseSum = _obj.ExpenseSum;
      assignment.ExpenseReportDate = _obj.ExpenseReportDate;
      
      assignment.Expenses.Clear();
      
      foreach(var expense in _obj.Expenses)
      {
        var assignmentExpense = assignment.Expenses.AddNew();
        assignmentExpense.ExpenseType = expense.ExpenseType;
        assignmentExpense.ExpenseSum = expense.ExpenseSum;
        assignmentExpense.ExpenseDescription = expense.ExpenseDescription;
      }
      
    }

    public virtual void CompleteAssignment6(DirRX.ExpenseReports.IExpenseRequestReWorkAssignment assignment, DirRX.ExpenseReports.Server.ExpenseRequestReWorkAssignmentArguments e)
    {
      // Скопировать значения свойств в задачу
      _obj.Purpose = assignment.Purpose;
      _obj.ExpenseSum = assignment.ExpenseSum;
      _obj.ExpenseReportDate = assignment.ExpenseReportDate;
      
      _obj.Expenses.Clear();
      
      foreach(var expense in assignment.Expenses)
      {
        var taskExpense = _obj.Expenses.AddNew();
        taskExpense.ExpenseType = expense.ExpenseType;
        taskExpense.ExpenseSum = expense.ExpenseSum;
        taskExpense.ExpenseDescription = expense.ExpenseDescription;
      }
      
      // Переформировать тему задачи, т.к. данные командировки могли поменяться.
      Functions.ExpenseRequestApprovalTask.UpdateTaskSubject(_obj);
    }
    
    #endregion
    
    #region Уведомление об отказе (блок 7)
    
    public virtual void StartBlock7(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseRequestApprovalTasks.Resources.RefuseNotificationSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    #endregion
    
    #region Деньги перечислены (блок 8)
    
    public virtual void StartBlock8(Sungero.Workflow.Server.NoticeArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseRequestApprovalTasks.Resources.ApproveNotificationSubjectTemplateFormat(_obj.ExpenseSum, _obj.Purpose);
    }
    
    #endregion
    
    #region Ожидание даты отчета (блок 13)
    
    public virtual bool Monitoring13Result()
    {
      return Calendar.Today >= _obj.ExpenseReportDate;
    }
    
    #endregion
    
    #region Создание авансового отчета (блок 9)

    public virtual void Script9Execute()
    {
      // Создать авансовый отчет
      var document = DirRX.ExpenseReports.PublicFunctions.Module.Remote.CreateExpenseReport(_obj.Employee, _obj.Purpose, _obj.ExpenseSum.Value);
      
      // Перенести информацию о расходах в авансовый отчет
      foreach(var requestExpense in _obj.Expenses)
      {
        var expense = document.Expenses.AddNew();
        expense.ExpenseType = requestExpense.ExpenseType;
        expense.ExpenseSum = requestExpense.ExpenseSum.Value;
        expense.ExpenseDate = _obj.ExpenseReportDate;
        expense.ExpenseDescription = requestExpense.ExpenseDescription;
      }
      document.Save();
      
      // Стартовать задачу на согласование.
      var expRepTask = DirRX.ExpenseReports.ExpenseReportApprovalTasks.Create();
      expRepTask.ExpenseReportGroup.ExpenseReports.Add(document);
      expRepTask.PrepareDeadline = _obj.ExpenseReportDate.Value;
      
      // Вложить текущую задачу по заявке
      expRepTask.OtherGroup.All.Add(_obj);
      
      // Вложить прочие влодения из текущей задачи
      foreach (var att in _obj.AllAttachments)
        expRepTask.OtherGroup.All.Add(att);

      expRepTask.Start();

    }

    #endregion
  }
}