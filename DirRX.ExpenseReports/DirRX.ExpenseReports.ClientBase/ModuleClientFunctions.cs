using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using Sungero.Company;
using Sungero.Domain;
using DocflowParamKeys = DirRX.ExpenseReports.Constants.Module.DocflowParamKeys;
using System.IO;

namespace DirRX.ExpenseReports.Client
{
  public class ModuleFunctions
  {

    #region Работа с захватом подтверждающих документов с почты.

    /// <summary>
    /// Обработать пакет подтверждающих документов с почты с использованием Ario (SmartProcessing).
    /// </summary>
    /// <param name="senderLineName">Наименование линии.</param>
    /// <param name="instanceInfosXmlPath">Путь к xml файлу DCS c информацией об экземплярах захвата и о захваченных файлах.</param>
    /// <param name="deviceInfoXmlPath">Путь к xml файлу DCS c информацией об устройствах ввода.</param>
    /// <param name="inputFilesXmlPath">Путь к xml файлу DCS c информацией об импортируемых файлах.</param>
    /// <param name="packageFolderPath">Путь к папке хранения файлов, переданных в пакете.</param>
    public virtual void SmartProcessCapturedPackage(string senderLineName, string instanceInfosXmlPath,
                                                    string deviceInfoXmlPath, string inputFilesXmlPath,
                                                    string packageFolderPath)
    {
      ProcessCapturedPackageBase(senderLineName, instanceInfosXmlPath,
                                 deviceInfoXmlPath, inputFilesXmlPath,
                                 packageFolderPath, true);
    }
    
    /// <summary>
    /// Обработать пакет документов c почты без Ario.
    /// </summary>
    /// <param name="senderLineName">Наименование линии.</param>
    /// <param name="instanceInfosXmlPath">Путь к xml файлу DCS c информацией об экземплярах захвата и о захваченных файлах.</param>
    /// <param name="deviceInfoXmlPath">Путь к xml файлу DCS c информацией об устройствах ввода.</param>
    /// <param name="inputFilesXmlPath">Путь к xml файлу DCS c информацией об импортируемых файлах.</param>
    /// <param name="packageFolderPath">Путь к папке хранения файлов, переданных в пакете.</param>
    public virtual void ProcessCapturedPackage(string senderLineName, string instanceInfosXmlPath,
                                               string deviceInfoXmlPath, string inputFilesXmlPath,
                                               string packageFolderPath)
    {
      ProcessCapturedPackageBase(senderLineName, instanceInfosXmlPath,
                                 deviceInfoXmlPath, inputFilesXmlPath,
                                 packageFolderPath, false);
    }
    
    /// <summary>
    /// Обработать пакет подтверждающих документов с почты с использованием Ario или без него.
    /// </summary>
    /// <param name="senderLineName">Наименование линии.</param>
    /// <param name="instanceInfosXmlPath">Путь к xml файлу DCS c информацией об экземплярах захвата и о захваченных файлах.</param>
    /// <param name="deviceInfoXmlPath">Путь к xml файлу DCS c информацией об устройствах ввода.</param>
    /// <param name="inputFilesXmlPath">Путь к xml файлу DCS c информацией об импортируемых файлах.</param>
    /// <param name="packageFolderPath">Путь к папке хранения файлов, переданных в пакете.</param>
    public virtual void ProcessCapturedPackageBase(string senderLineName, string instanceInfosXmlPath,
                                                   string deviceInfoXmlPath, string inputFilesXmlPath,
                                                   string packageFolderPath, bool useArio)
    {
      // функция отключена из-за несовместимости с Rx 4.1
      // логику функции надо переносить в плагин для rxcmd
      /*
      // HACK Переопределить имя линии. Фиктивная настройка создаётся при инициализации
      senderLineName = DirRX.ExpenseReports.Constants.Module.LineName;
      
      var blobPackage = Sungero.SmartProcessing.PublicFunctions.Module.PrepareDcsPackage(senderLineName, instanceInfosXmlPath, deviceInfoXmlPath,
                                                                                         inputFilesXmlPath, packageFolderPath);
      if (blobPackage.SourceType != Sungero.SmartProcessing.BlobPackage.SourceType.Mail)
        throw new ApplicationException(Resources.CaptureFromFolderUnavailableError);
      
      // HACK Чтобы не копипастить кучу функций SmartProcessing, просто убрать информацию о теле письма.
      blobPackage.MailBodyBlob = null;
      
      if (blobPackage.SourceType == Sungero.SmartProcessing.BlobPackage.SourceType.Mail
          && !blobPackage.Blobs.Any())
        throw new ApplicationException(Resources.EmptyMailPackage);

      // Проверить, есть ли сотрудник с таким email, если нет, обработка сразу прекратится с ошибкой.
      DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetEmpoyeeByEmail(blobPackage.FromAddress);
      
      if (useArio)
        Sungero.SmartProcessing.PublicFunctions.Module.ProcessPackageInArio(blobPackage);
      
      DirRX.ExpenseReports.PublicFunctions.Module.Remote.ProcessCapturedPackage(blobPackage);
      */
    }
    
    #endregion
    
    /// <summary>
    /// Создать новую задачу на согласование заявки на аванс.
    /// </summary>
    public virtual void CreateNewExpenseRequestTask()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.CreateExpenseRequestTask().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }
    
    /// <summary>
    /// Выбор значений настроек модуля.
    /// </summary>
    [Public]
    public virtual void SelectModuleSettings()
    {
      // Менять настройки может только администратор
      if (Users.Current.IncludedIn(Roles.Administrators))
      {
        var dialog = Dialogs.CreateInputDialog(Resources.ModuleSettings);
        
        // Спец. почтовый ящик.
        var supportingDocsMail = Functions.Module.Remote.GetDocflowParamValue(DocflowParamKeys.SupportingDocsMailKey);
        var dialogMailSelect = dialog.AddString(Resources.MailSelect, false, supportingDocsMail);
        
        if (dialog.Show() == DialogButtons.Ok)
        {
          if (dialogMailSelect.Value != supportingDocsMail)
            Functions.Module.Remote.AddDocflowParam(DocflowParamKeys.SupportingDocsMailKey, dialogMailSelect.Value);
        }
      }
      else
        Dialogs.ShowMessage(DirRX.BusinessTrips.Resources.ModuleSettingsAccessErrorText, MessageType.Warning);
    }

    /// <summary>
    /// Показать все мои авансовые отчеты.
    /// </summary>
    [Public]
    public virtual void ShowMyExpenseReports()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.GetMyExpenseReports().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }


    /// <summary>
    /// Подписать документ.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="documents">Документ.</param>
    /// <param name="comment">Комментарий.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <param name="EndorseWhenCantApprove">Использовать простую электронную подпись, если нет возможности подписания сертификатом ЭП.</param>
    [Public]
    public static void ApproveDocument(IAssignment assignment, IOfficialDocument document, string comment, Sungero.Core.IValidationArgs eventArgs, bool EndorseWhenCantApprove)
    {
      var currentEmployee = Employees.Current;
      var performer = Employees.As(assignment.Performer);
      
      var activeText = string.IsNullOrWhiteSpace(assignment.ActiveText) ? comment : assignment.ActiveText;
      var signatories = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetEmployeeSignatories(document);
      var signatory = signatories.Contains(performer.Id) && signatories.Contains(currentEmployee.Id) ? performer : currentEmployee;
      
      ApproveDocument(document, activeText, signatory, eventArgs, EndorseWhenCantApprove);
    }

    /// <summary>
    /// Подписать документ.
    /// </summary>
    /// <param name="documents">Документ.</param>
    /// <param name="comment">Комментарий.</param>
    /// <param name="signatory">Подписант.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <param name="EndorseWhenCantApprove">Использовать простую электронную подпись, если нет возможности подписания сертификатом ЭП.</param>
    [Public]
    public static void ApproveDocument(IOfficialDocument document, string comment, IEmployee signatory, Sungero.Core.IValidationArgs eventArgs, bool EndorseWhenCantApprove)
    {
      var haveError = false;
      var canApprove = true;
      var validate = Functions.Module.Remote.ValidateBeforeSign(document);
      
      foreach (var error in validate.Errors)
        eventArgs.AddError(error);
      
      haveError = validate.Errors.Any();
      
      if (haveError)
        return;
      
      if (!validate.CanApprove)
        canApprove = false;
      
      // Подписание утверждающей подписью.
      if (canApprove)
      {
        try
        {
          if (!Sungero.Docflow.PublicFunctions.Module.ApproveWithAddenda(document, new List<IOfficialDocument>(), null, signatory, false, false, comment))
            eventArgs.AddError(ApprovalTasks.Resources.ToPerformNeedSignDocument);

        }
        catch (CommonLibrary.Exceptions.PlatformException ex)
        {
          if (!ex.IsInternal)
            eventArgs.AddError(ex.Message.Trim().EndsWith(".") ? ex.Message : string.Format("{0}.", ex.Message));
          else
            throw;
        }
      }
      else
      {
        if (EndorseWhenCantApprove)
        {
          if (!Sungero.Docflow.PublicFunctions.Module.EndorseWithAddenda(document, new List<Sungero.Content.IElectronicDocument>(), null, signatory, false, comment))
            eventArgs.AddError(ApprovalTasks.Resources.ToPerformNeedSignDocument);
        }
        else
        {
          if (!document.AccessRights.CanApprove())
            eventArgs.AddError(Sungero.Docflow.ApprovalSigningAssignments.Resources.NoRigthToApproveDocumentForSubstituteFormat(Employees.Current.Name, signatory));
          else
            eventArgs.AddError(Sungero.Docflow.Resources.NoRightsToApproveDocument);
        }
      }
    }

    /// <summary>
    /// Создать авансовый отчет.
    /// </summary>
    [Public]
    public virtual void CreateNewExpenseReport()
    {
      if (Sungero.Company.Employees.Current != null)
        Functions.Module.Remote.CreateNewExpenseReport().Show();
      else
        Dialogs.ShowMessage(Sungero.Docflow.Resources.CantSendTaskByNonEmployee);
    }

    /// <summary>
    /// Запустить выгрузку с поиском документов.
    /// </summary>
    [Public]
    public virtual void ExportExpenseReportDialogWithSearchInWeb()
    {
      int zipModelFilesCount = 0;
      double filesSumSize = 0;
      long zipModelFilesSumSize = 0;
      string addErrorMessage = string.Empty;

      var errorInExportFlag = false;
      
      Structures.Module.ExportResults exportResult = null;
      DirRX.ExpenseReports.Structures.Module.IExportDialogSearch filter = null;
      var docsForExport = new List<Structures.Module.ExportedDocument>();
      
      var firstStepFlag = true;
      var secondStepFlag = false;
      var endStepFlag = false;

      var dialog = Dialogs.CreateInputDialog(Resources.ExportDialog_Title);
      dialog.Height = 210;
      
      // HACK: Принудительно увеличиваем ширину диалога для корректного отображения кнопок.
      var fakeControl = dialog.AddString("123456789012345", false);
      fakeControl.IsVisible = false;
      
      var properties = ExpenseReports.Info.Properties;
      
      // Свойства для фильтрации
      var dateFrom = dialog.AddDate(Resources.ExportDialog_Search_DateFrom, true, Calendar.BeginningOfYear(Calendar.Now));
      var dateTo = dialog.AddDate(Resources.ExportDialog_Search_DateTo, true, Calendar.EndOfYear(Calendar.Now));
      var unit = dialog.AddSelect(properties.BusinessUnit.LocalizedName, true, Sungero.Company.BusinessUnits.Null);
      var employee = dialog.AddSelect(properties.Employee.LocalizedName, false, Sungero.Company.Employees.Null);
      
      var showDocs = dialog.Buttons.AddCustom(Resources.ExportDialog_Search_Show);
      showDocs.Name = Resources.ExportDialog_Search_Show;
      
      var next = dialog.Buttons.AddCustom(Resources.ExportDialog_Next);
      var back = dialog.Buttons.AddCustom(Resources.ExportDialog_Back);
      var startExport = dialog.Buttons.AddCustom(Resources.ExportDialog_StartExport);
      var cancel = dialog.Buttons.AddCustom(Resources.ExportDialog_Close);
      
      // Фильтрация работников по НОР.
      unit.SetOnValueChanged(u =>
                             {
                               if (u.NewValue != null && employee.Value != null && !Equals(employee.Value.Department.BusinessUnit, u.NewValue))
                                 employee.Value = null;
                               
                               // Отфильтровать сотрудников по нашей орг.
                               var unitEmployees = Functions.Module.Remote.GetEmployeesByBusinessUnit(u.NewValue);
                               employee = employee.From(unitEmployees);
                             });
      
      // Заполнить Нашу орг. значением по умочнанию после определения события, иначе фильтрация сотрудников работает некорректно
      unit.Value = Sungero.Company.Employees.Current.Department.BusinessUnit;
      
      employee.SetOnValueChanged(e =>
                                 {
                                   if (e.NewValue != null && unit.Value == null)
                                     unit.Value = e.NewValue.Department.BusinessUnit;
                                 });
      
      Action<CommonLibrary.InputDialogRefreshEventArgs> refresh = (r) =>
      {
        if (firstStepFlag)
        {
          dialog.Text = Resources.ExportDialog_Step_Search;
          dialog.Text += Environment.NewLine;
          
          // Проверить введённые даты
          if (dateFrom.Value > dateTo.Value)
            r.AddError(Resources.ExportDialog_DatesErrorFormat(Resources.ExportDialog_Search_DateFrom, Resources.ExportDialog_Search_DateTo));
        }
        else if (secondStepFlag)
        {
          dialog.Text = Resources.ExportDialog_Step_DocsCount_Web;
          dialog.Text += Environment.NewLine + Environment.NewLine;
          dialog.Text += Resources.ExportDialog_TotalForDownloadFormat(docsForExport.Count, filesSumSize.ToString("0.#"));
        }
        else if (endStepFlag)
        {
          dialog.Text = Resources.ExportDialog_Step_End_Web;
          dialog.Text += Environment.NewLine + Environment.NewLine;
          dialog.Text += Resources.ExportDialog_End_AllDocsFormat(zipModelFilesCount);

          if (errorInExportFlag)
          {
            dialog.Text += Environment.NewLine;
            dialog.Text += Resources.ExportDialog_End_NotExportedDocsFormat(exportResult.ExportedDocuments.Count(d => d.IsFaulted));
          }
        }
        
        unit.IsVisible = firstStepFlag;
        dateFrom.IsVisible = firstStepFlag;
        dateTo.IsVisible = firstStepFlag;
        employee.IsVisible = firstStepFlag;
        
        showDocs.IsVisible = secondStepFlag;
        next.IsVisible = firstStepFlag;
        back.IsVisible = secondStepFlag;
        startExport.IsVisible = !firstStepFlag;
        
        startExport.IsEnabled = (addErrorMessage == String.Empty);
        
        cancel.Name = endStepFlag ? Resources.ExportDialog_Close : Resources.ExportDialog_Cancel;
        startExport.Name = secondStepFlag ? Resources.ExportDialog_StartExport : Resources.ExportDialog_Download;
      };
      dialog.SetOnRefresh(refresh);
      
      IZip zip = null;
      
      dialog.SetOnButtonClick(
        (h) =>
        {
          if (h.Button != cancel)
            h.CloseAfterExecute = false;
          
          // Кнопка Открыть документы
          if (secondStepFlag && h.Button == showDocs && h.IsValid)
          {
            filter = DirRX.ExpenseReports.Structures.Module.ExportDialogSearch
              .Create(dateFrom.Value, dateTo.Value, unit.Value, employee.Value);
            Functions.Module.Remote.SearchExpenseReportDocsByRequisites(filter, true).ShowModal(Resources.ExportedDocsListName);
          }
          
          if (h.Button == cancel)
          {
            unit.IsRequired = false;
            dateTo.IsRequired = false;
            dateFrom.IsRequired = false;
          }
          
          // Кнопка Скачать
          if (h.Button == startExport && endStepFlag)
          {
            zip.Export();
          }
          
          // Кнопка Далее
          if (h.Button == next && h.IsValid && firstStepFlag)
          {
            addErrorMessage = String.Empty;
            try
            {
              filter = DirRX.ExpenseReports.Structures.Module.ExportDialogSearch
                .Create(dateFrom.Value, dateTo.Value, unit.Value, employee.Value);
              docsForExport = Functions.Module.Remote.PrepareExportExpenseReportDialogDocuments(filter);
            }
            catch (Exception ex)
            {
              addErrorMessage = Resources.ExportDialog_Error_Client_NoReason_Web;
              Logger.Error(addErrorMessage, ex);
              h.AddError(addErrorMessage);
              return;
            }

            exportResult = Functions.Module.Remote.PrepareBodiesAndSigns(docsForExport);
            
            zipModelFilesCount = exportResult.ZipModels.Count;
            zipModelFilesSumSize = exportResult.ZipModels.Sum(m => m.Size);
            var zipModelFilesSumSizeMB = zipModelFilesSumSize / Constants.Module.ConvertMb;
            errorInExportFlag = exportResult.ExportedDocuments.Any(d => d.IsFaulted);
            
            if (zipModelFilesCount == 0)
            {
              addErrorMessage = Resources.NoDocsForExportWarning;
              h.AddError(addErrorMessage);
              return;
            }
            else if (zipModelFilesCount > Constants.Module.ExportedFilesCountMaxLimit)
            {
              addErrorMessage = Resources.ExportDialog_Error_ExportedFilesLimitFormat(zipModelFilesCount, Constants.Module.ExportedFilesCountMaxLimit);
              h.AddError(addErrorMessage);
              return;
            }
            else if (zipModelFilesSumSizeMB > Constants.Module.ExportedFilesSizeMaxLimitMb)
            {
              addErrorMessage = Resources.ExportDialog_Error_ExportedSizeLimitFormat(zipModelFilesSumSizeMB, Constants.Module.ExportedFilesSizeMaxLimitMb);
              h.AddError(addErrorMessage);
              return;
            }
            filesSumSize = (double)zipModelFilesSumSize / Constants.Module.ConvertMb;
            if (filesSumSize < 0.1)
              filesSumSize = 0.1;
            
            // Если ошибок не было, перейти на следующий шаг
            firstStepFlag = false;
            secondStepFlag = true;
            refresh.Invoke(null);
          }
          
          // Кнопка Назад
          if (h.Button == back && secondStepFlag)
          {
            firstStepFlag = true;
            secondStepFlag = false;
            refresh.Invoke(null);
          }
          
          // Кнопка Выгрузить
          if (h.Button == startExport && h.IsValid && secondStepFlag)
          {
            Logger.DebugFormat("Export Expense reports (Zip). Files count = {0}, files size = {1} MB", zipModelFilesCount, filesSumSize.ToString("0.#"));
            
            if (exportResult.ZipModels != null && exportResult.ZipModels.Any() && exportResult.ExportedDocuments != null && exportResult.ExportedDocuments.Any())
              zip = Functions.Module.Remote.CreateZipFromZipModel(exportResult.ZipModels, exportResult);

            secondStepFlag = false;
            endStepFlag = true;
            refresh.Invoke(null);
          }
          
        });
      dialog.Show();
    }

  }
}