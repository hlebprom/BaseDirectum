using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using DocKind = DirRX.BusinessTrips.Constants.Module.DocumentKind;
using BusinessTripRoleGuids = DirRX.BusinessTrips.Constants.Module.BusinessTripRoleGuids;
using DocflowParamKeys = DirRX.BusinessTrips.Constants.Module.DocflowParamKeys;

namespace DirRX.BusinessTrips.Server
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
    }
    
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      InitializationLogger.Debug("Init: Create roles.");
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameAccountant, Resources.RoleDescrAccountant, BusinessTripRoleGuids.Accountant);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameTiketsResponsible, Resources.RoleDescrTiketsResponsible, BusinessTripRoleGuids.TiketsResponsible);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Resources.RoleNameOrdersSigner, Resources.RoleDescrOrdersSigner, BusinessTripRoleGuids.OrderSigner);
    }
    
    /// <summary>
    /// Создать типы документов для командировок.
    /// </summary>
    public virtual void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types.");
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.BusinessTrips.Resources.OrderTypeName, BusinessTripOrder.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.BusinessTrips.Resources.MemoTypeName, BusinessTripMemo.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
    }
    
    /// <summary>
    /// Создать виды документов для командировок.
    /// </summary>
    public virtual void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init: Create document kinds.");

      var numerable = Sungero.Docflow.DocumentKind.NumberingType.Numerable;
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      
      // Приказ на командировку.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.BusinessTrips.Resources.BusinessTripOrderKindName,
                                                                              DirRX.BusinessTrips.Resources.BusinessTripOrderKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, true, BusinessTripOrder.ClassTypeGuid, null,
                                                                              DocKind.BusinessTripOrderKind);
      
      // Приказ на изменение командировки.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.BusinessTrips.Resources.ChangeBusinessTripOrderKindName,
                                                                              DirRX.BusinessTrips.Resources.ChangeBusinessTripOrderKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, true, BusinessTripOrder.ClassTypeGuid, null,
                                                                              DocKind.BusinessTripChangeOrderKind);
      
      // Служебная записка.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.BusinessTrips.Resources.BusinessTripMemoKindName,
                                                                              DirRX.BusinessTrips.Resources.BusinessTripMemoKindName, registrable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, false, BusinessTripMemo.ClassTypeGuid, null,
                                                                              DocKind.BusinessTripMemoKind);
      // Приказ об отзыве из командировки.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.BusinessTrips.Resources.BusinessTripRecallOrderKindName,
                                                                              DirRX.BusinessTrips.Resources.BusinessTripRecallOrderKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, true, BusinessTripOrder.ClassTypeGuid, null,
                                                                              DocKind.BusinessTripRecallOrderKind);
      // Приказ об отмене командировки.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(DirRX.BusinessTrips.Resources.BusinessTripCancellOrderKindName,
                                                                              DirRX.BusinessTrips.Resources.BusinessTripCancellOrderKindName, numerable,
                                                                              Sungero.Docflow.DocumentRegister.DocumentFlow.Inner, true, true, BusinessTripOrder.ClassTypeGuid, null,
                                                                              DocKind.BusinessTripCancelOrderKind);
    }
    
    /// <summary>
    /// Выдача прав на объекты модуля.
    /// </summary>
    public virtual void GrantRights()
    {
      InitializationLogger.Debug("Init: Grant rights.");
      
      var accountantRole = Roles.GetAll(g => g.Sid == BusinessTripRoleGuids.Accountant).FirstOrDefault();
      var orderSignerRole = Roles.GetAll(g => g.Sid == BusinessTripRoleGuids.OrderSigner).FirstOrDefault();
      
      #region Выдача прав на документы.
      
      DirRX.BusinessTrips.BusinessTripMemos.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.BusinessTrips.BusinessTripMemos.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Approve);
      DirRX.BusinessTrips.BusinessTripMemos.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.BusinessTrips.BusinessTripMemos.AccessRights.Save();
      
      DirRX.BusinessTrips.BusinessTripOrders.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Approve);
      DirRX.BusinessTrips.BusinessTripOrders.AccessRights.Grant(orderSignerRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.BusinessTrips.BusinessTripOrders.AccessRights.Save();
      
      #endregion
      
      #region Выдача прав на задачи.
      
      DirRX.BusinessTrips.BusinessTripApprovalTasks.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.BusinessTrips.BusinessTripApprovalTasks.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.BusinessTrips.BusinessTripApprovalTasks.AccessRights.Save();
      
      DirRX.BusinessTrips.BusinessTripCancelTasks.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.BusinessTrips.BusinessTripCancelTasks.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.BusinessTrips.BusinessTripCancelTasks.AccessRights.Save();
      
      DirRX.BusinessTrips.BusinessTripRecallTasks.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      DirRX.BusinessTrips.BusinessTripRecallTasks.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      DirRX.BusinessTrips.BusinessTripRecallTasks.AccessRights.Save();
      #endregion
      
      #region Выдача прав на справочники.
      
      DirRX.BusinessTrips.BusinessTrips.AccessRights.Grant(accountantRole, DefaultAccessRightsTypes.FullAccess);
      
      // Для возможности синхронизации значений выдать всем права на изменение. При открытии карточки поля блокируются в коде
      DirRX.BusinessTrips.BusinessTrips.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Change);
      DirRX.BusinessTrips.BusinessTrips.AccessRights.Save();
      
      #endregion
      
    }
    
    /// <summary>
    /// Создание Суточных и Проживания и их указание в настройках модуля.
    /// </summary>
    public virtual void CreateExpenseTypeRecords()
    {
      InitializationLogger.Debug("Init: Create Expense types.");
      
      // Суточные.
      var perDiemExpenseType = DirRX.ExpenseReports.PublicFunctions.ExpenseType.CreateAndGetExpenseType(DirRX.BusinessTrips.Resources.ExpenseTypePerDiem);
      
      var perDiemExpenseTypeId = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(DocflowParamKeys.PerDiemExpenseTypeIdKey);
      if (string.IsNullOrEmpty(perDiemExpenseTypeId))
        DirRX.ExpenseReports.PublicFunctions.Module.Remote.AddDocflowParam(DocflowParamKeys.PerDiemExpenseTypeIdKey, perDiemExpenseType.Id.ToString());
      
      
      // Проживание.
      var hotelExpenseType = DirRX.ExpenseReports.PublicFunctions.ExpenseType.CreateAndGetExpenseType(DirRX.BusinessTrips.Resources.ExpenseTypeHotel);
      
      var hotelExpenseTypeId = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocflowParamValue(DocflowParamKeys.HotelExpenseTypeIdKey);
      if (string.IsNullOrEmpty(hotelExpenseTypeId))
        DirRX.ExpenseReports.PublicFunctions.Module.Remote.AddDocflowParam(DocflowParamKeys.HotelExpenseTypeIdKey, hotelExpenseType.Id.ToString());
    }
    
  }

}