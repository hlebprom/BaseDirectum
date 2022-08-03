using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.ExpenseReports.ExpenseReportApprovalTask;

namespace DirRX.ExpenseReports.Server
{
  partial class ExpenseReportApprovalTaskRouteHandlers
  {
    #region Требуется заполнение авансового отчета? (блок 3)
    
    public virtual bool Decision3Result()
    {
      return _obj.NeedPrepare.Value;
    }
    
    #endregion
    
    #region Заполнение авансового отчета (блок 4)
    public virtual void StartBlock4(DirRX.ExpenseReports.Server.ExpenseReportPrepareAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.PrepareAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
      e.Block.IsRework = false;
      if (_obj.PrepareDeadline.HasValue)
        e.Block.AbsoluteDeadline = _obj.PrepareDeadline.Value;
    }
    
    public virtual void CompleteAssignment4(DirRX.ExpenseReports.IExpenseReportPrepareAssignment assignment, DirRX.ExpenseReports.Server.ExpenseReportPrepareAssignmentArguments e)
    {
      Functions.ExpenseReportApprovalTask.AttachSupportingDocs(_obj);
    }
    
    #endregion
    
    #region Согласование руководителем (блок 13)
    public virtual void StartBlock13(DirRX.ExpenseReports.Server.ExpenseReportApprovalAssignmentArguments e)
    {
      var manager = Functions.Module.GetManager(_obj.Employee);
      if (manager != null)
      {
        e.Block.Performers.Add(manager);
        
        // Сформировать тему задачи
        Functions.ExpenseReportApprovalTask.UpdateTaskSubject(_obj);
        
        // Сформировать тему задания
        e.Block.Subject = ExpenseReportApprovalTasks.Resources.ApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
        
        // Поменять состояние документа
        _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault().ExpenseReportStatus = DirRX.ExpenseReports.ExpenseReport.ExpenseReportStatus.Agreement;
      }
    }
    #endregion
    
    #region Согласование бухгалтером (блок 14)
    public virtual void StartBlock14(DirRX.ExpenseReports.Server.ExpenseReportApprovalAssignmentArguments e)
    {
      var accountant = Functions.Module.GetRoleRecipientForBusinessUnit(DirRX.ExpenseReports.Constants.Module.ExpenseReportRoleGuids.Accountant,
                                                                        Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(accountant);
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.ApprovalAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    #endregion
    
    #region Генерация авансового отчета из шаблона (блок 16)
    public virtual void Script16Execute()
    {
      Functions.ExpenseReport.CreateFromTemplate(_obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault());
    }
    #endregion
    
    #region Подписание авансового отчета сотрудником (блок 15)
    public virtual void StartBlock15(DirRX.ExpenseReports.Server.ExpenseReportEmployeeSignAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.SignAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    #endregion
    
    #region Подписание руководителем и бухгалтером (блок 5)
    public virtual void StartBlock5(DirRX.ExpenseReports.Server.ExpenseReportApprovalSignAssignmentArguments e)
    {
      e.Block.Performers.Add(Functions.Module.GetManager(_obj.Employee));
      
      var accountant = Functions.Module.GetRoleRecipientForBusinessUnit(DirRX.ExpenseReports.Constants.Module.ExpenseReportRoleGuids.Accountant,
                                                                        Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(accountant);
      
      // Сформировать тему задания
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.SignAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void EndBlock5(DirRX.ExpenseReports.Server.ExpenseReportApprovalSignAssignmentEndBlockEventArguments e)
    {
      // Поменять состояние документа
      _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault().ExpenseReportStatus = DirRX.ExpenseReports.ExpenseReport.ExpenseReportStatus.Approved;
    }
    #endregion

    #region Доработка авансового отчета (блок 11)
    public virtual void StartBlock11(DirRX.ExpenseReports.Server.ExpenseReportPrepareAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.ReWorkAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
      e.Block.IsRework = true;
      
      var expRep = _obj.ExpenseReportGroup.ExpenseReports.FirstOrDefault();
      if (expRep.HasVersions)
        expRep.Versions.Clear();
      
      // Поменять состояние документа
      expRep.ExpenseReportStatus = DirRX.ExpenseReports.ExpenseReport.ExpenseReportStatus.Start;
    }
    
    public virtual void CompleteAssignment11(DirRX.ExpenseReports.IExpenseReportPrepareAssignment assignment, DirRX.ExpenseReports.Server.ExpenseReportPrepareAssignmentArguments e)
    {
      Functions.ExpenseReportApprovalTask.AttachSupportingDocs(_obj);
    }
    #endregion
    
    #region Предоставление оригиналов (блок 8)
    public virtual void StartBlock8(DirRX.ExpenseReports.Server.ExpenseReportSimpleAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Employee);
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.SubmittingOriginalsAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void StartAssignment8(DirRX.ExpenseReports.IExpenseReportSimpleAssignment assignment, DirRX.ExpenseReports.Server.ExpenseReportSimpleAssignmentArguments e)
    {
      _obj.OriginalDocsAssignment = assignment;
    }
    #endregion
    
    #region Контроль возврата оригиналов (блок 7)
    
    public virtual void StartBlock7(DirRX.ExpenseReports.Server.ExpenseReportSimpleAssignmentArguments e)
    {
      var accountant = Functions.Module.GetRoleRecipientForBusinessUnit(DirRX.ExpenseReports.Constants.Module.ExpenseReportRoleGuids.Accountant,
                                                                        Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee));
      e.Block.Performers.Add(accountant);
      e.Block.Subject = ExpenseReportApprovalTasks.Resources.GetOriginalsAssignmentSubjectTemplateFormat(_obj.CommonSubjectPart);
    }
    
    public virtual void EndBlock7(DirRX.ExpenseReports.Server.ExpenseReportSimpleAssignmentEndBlockEventArguments e)
    {
      if (_obj.OriginalDocsAssignment.Result == null)
        _obj.OriginalDocsAssignment.Complete(DirRX.ExpenseReports.ExpenseReportSimpleAssignment.Result.Complete);
    }
    
    #endregion
  }
}
