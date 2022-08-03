using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using DocKind = DirRX.ExpenseReports.Constants.Module.DocumentKindGuids;
using ExpenseReportRoleGuids = DirRX.ExpenseReports.Constants.Module.ExpenseReportRoleGuids;
using ArioClassNames = DirRX.ExpenseReports.Constants.Module.NewArioClassNames;
using ArioGrammarNames = DirRX.ExpenseReports.Constants.Module.NewArioGrammarNames;

namespace DirRX.ExpenseReports.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateRoles();
      CreateDocumentTypes();
      CreateDocumentKinds();
      GrantRights();
      CreateExpenseTypeRecords();
      FillSmartProcessingSettings();
    }
    
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      InitializationLogger.Debug("Init: Create roles.");
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameAccountant, Resources.RoleDescrAccountant, ExpenseReportRoleGuids.Accountant);
    }

    /// <summary>
    /// Создать типы документов для авансовых отчетов.
    /// </summary>
    public virtual void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types.");
      
      // Авансовые отчеты
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.ExpenseReports.Resources.ExpenseReportTypeName, ExpenseReport.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
      
      // Подтверждающие документы
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.ExpenseReports.Resources.SupportingDocumentTypeName, SupportingDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
    }
    
    /// <summary>
    /// Создать виды документов для авансовых отчетов.
    /// </summary>
    public virtual void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init: Create document kinds.");

      var numerable = Sungero.Docflow.DocumentKind.NumberingType.Numerable;
      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      
      // Авансовый отчет.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.ExpenseReports.Resources.ExpenseReportKindName,
                                                                              DirRX.ExpenseReports.Resources.ExpenseReportKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, true, ExpenseReport.ClassTypeGuid, null,
                                                                              DocKind.ExpenseReportKind);
      
      // Авиабилет.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.ExpenseReports.Resources.AviaKindName,
                                                                              DirRX.ExpenseReports.Resources.AviaKindName, notNumerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, SupportingDocument.ClassTypeGuid, null,
                                                                              DocKind.AviaKind);
      
      // Железнодорожный билет.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.ExpenseReports.Resources.RailwayTicketKindName,
                                                                              DirRX.ExpenseReports.Resources.RailwayTicketKindShortName, notNumerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, SupportingDocument.ClassTypeGuid, null,
                                                                              DocKind.RailwayTicketKind);
      // Кассовый чек.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.ExpenseReports.Resources.ReceiptKindName,
                                                                              DirRX.ExpenseReports.Resources.ReceiptKindName, notNumerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, SupportingDocument.ClassTypeGuid, null,
                                                                              DocKind.ReceiptKind);
      
      // Прочий подтверждающий документ.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.ExpenseReports.Resources.SupportingDocKindName,
                                                                              DirRX.ExpenseReports.Resources.SupportingDocKindShortName, notNumerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, SupportingDocument.ClassTypeGuid, null,
                                                                              DocKind.SupportingDocKind);
    }
    
    /// <summary>
    /// Выдача прав на объекты модуля.
    /// </summary>
    public virtual void GrantRights()
    {
      InitializationLogger.Debug("Init: Grant rights.");
      
      var accountantRole = Roles.GetAll(g => g.Sid == ExpenseReportRoleGuids.Accountant).FirstOrDefault();
      
      #region Выдача прав на документы.

      DirRX.ExpenseReports.ExpenseReports.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.ExpenseReports.ExpenseReports.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Approve);
      DirRX.ExpenseReports.ExpenseReports.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.ExpenseReports.ExpenseReports.AccessRights.Save();
      
      DirRX.ExpenseReports.SupportingDocuments.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.ExpenseReports.SupportingDocuments.AccessRights.Save();
      
      #endregion
      
      #region Выдача прав на задачи.
      
      DirRX.ExpenseReports.ExpenseReportApprovalTasks.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.ExpenseReports.ExpenseReportApprovalTasks.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.ExpenseReports.ExpenseReportApprovalTasks.AccessRights.Save();
      
      DirRX.ExpenseReports.ExpenseRequestApprovalTasks.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.ExpenseReports.ExpenseRequestApprovalTasks.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.ExpenseReports.ExpenseRequestApprovalTasks.AccessRights.Save();
      
      #endregion
      
      #region Выдача прав на справочники.
      
      DirRX.ExpenseReports.ExpenseTypes.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      DirRX.ExpenseReports.ExpenseTypes.AccessRights.Save();
      
      DirRX.ExpenseReports.CurrencyRates.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      DirRX.ExpenseReports.CurrencyRates.AccessRights.Save();
      
      DirRX.ExpenseReports.LimitSettings.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      DirRX.ExpenseReports.LimitSettings.AccessRights.Save();
      
      #endregion
      
    }
    
    /// <summary>
    /// Создание записей справочника Типы расходов.
    /// </summary>
    public virtual void CreateExpenseTypeRecords()
    {
      InitializationLogger.Debug("Init: Create Expense types.");
      
      // Задать список типов расходов.
      var expenseTypeNames = new List<string>();
      expenseTypeNames.Add(DirRX.ExpenseReports.Resources.ExpenseTypeAvia);
      expenseTypeNames.Add(DirRX.ExpenseReports.Resources.ExpenseTypeTrain);
      expenseTypeNames.Add(DirRX.ExpenseReports.Resources.ExpenseTypeRoad);
      expenseTypeNames.Add(DirRX.ExpenseReports.Resources.ExpenseTypeOther);
      
      // Создать записи справочника Типы расходов, если их ещё нет.
      foreach(var expenseTypeName in expenseTypeNames)
        DirRX.ExpenseReports.PublicFunctions.ExpenseType.CreateAndGetExpenseType(expenseTypeName);
    }
    
    /// <summary>
    /// Заполнить правила в настройках интеллектуальной обработки и добавить настройку линии.
    /// </summary>
    [Public]
    public virtual void FillSmartProcessingSettings()
    {
      var smartProcessingSettings = Sungero.Docflow.PublicFunctions.SmartProcessingSetting.GetSettings();
      if (smartProcessingSettings != null)
      {
        // HACK Добавить фиктивную настройку линии, чтобы при обработке не падала ошибка. Линия для авансовых отчетов не нужна.
        var lineName = DirRX.ExpenseReports.Constants.Module.LineName;
        if (!smartProcessingSettings.CaptureSources.Where(r => r.SenderLineName == lineName).Any())
        {
          var sourceSetting = smartProcessingSettings.CaptureSources.AddNew();
          sourceSetting.SenderLineName = lineName;
          sourceSetting.Responsible = Users.Current;
        }
        const string ModuleName = "DirRX.ExpenseReports";
        // Класс, грамматика, функция обработки.
        var processRules = new List<string[]>()
        {
          new[] { ArioClassNames.Receipt, string.Empty, "CreateReceipt" },
          new[] { ArioClassNames.AirTicket, ArioGrammarNames.AirTicket, "CreateAirTicket" },
          new[] { ArioClassNames.RailwayTicket, ArioGrammarNames.RailwayTicket, "CreateRailwayTicket" }
        };
        
        foreach (var processRule in processRules)
        {
          if (!smartProcessingSettings.ProcessingRules.Where(r => r.ClassName == processRule[0]).Any())
          {
            var ruleSetting = smartProcessingSettings.ProcessingRules.AddNew();
            ruleSetting.ClassName = processRule[0];
            ruleSetting.GrammarName = processRule[1];
            ruleSetting.ModuleName = ModuleName;
            ruleSetting.FunctionName = processRule[2];
          }
        }
        
        smartProcessingSettings.Save();
      }
    }
  }
}
