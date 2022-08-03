using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.ExpenseReports.ExpenseReport;
using Aspose.Words;
using System.Globalization;
using Sungero.Company;

namespace DirRX.ExpenseReports.Server
{
  partial class ExpenseReportFunctions
  {
    /// </summary>
    /// Получить свободные подтверждающие документы для прикрепления к авансовому отчету.
    /// </summary>
    /// <param name="expRep">Авансовый отчет.</param>
    /// <returns>Подтверждающие документы.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<ISupportingDocument> GetFreeSupportingDocs()
    {
      return SupportingDocuments.GetAll(d => Equals(Employees.Current, d.Author) &&
                                        !ExpenseReports.GetAll().Any(r => !Equals(r, _obj) && r.Expenses.Any(exp => Equals(d, exp.SupportingDoc)))
                                       );
    }
    
    /// <summary>
    /// Создать версию авансового отчета из шаблона
    /// </summary>
    /// <param name="expenseReportTemplate">Шаблон авансового отчета</param>
    [Remote]
    public virtual void CreateFromTemplate()
    {
      var expenseReportTemplate = PublicFunctions.Module.GetTemplateForExpenseReport();

      using (var body = expenseReportTemplate.LastVersion.Body.Read())
      {
        var newVersion = _obj.CreateVersionFrom(body, expenseReportTemplate.AssociatedApplication.Extension);
        
        var exEntity = (Sungero.Domain.Shared.IExtendedEntity)_obj;
        exEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = expenseReportTemplate.Id;
        
        // HACK Без этого промежуточного сохранения таблица не заполняется
        _obj.Save();
        
        var asposeEpenseReport = Functions.Module.CreateAsposeWordDocument(newVersion);
        
        // Найти таблицу расходов в документе и заполнить её
        var tables = asposeEpenseReport.GetChildNodes(NodeType.Table, true).ToArray();
        var lastTable = tables.OfType<Aspose.Words.Tables.Table>().Last();
        if (lastTable != null)
        {
          var rowNumber = 1;

          foreach (var expenseRow in _obj.Expenses)
          {
            if (expenseRow.ExpenseSum.HasValue)
            {
              var newRow = new Aspose.Words.Tables.Row(asposeEpenseReport);
              
              // Номер по порядку
              AddCellToRow(asposeEpenseReport, newRow, rowNumber.ToString());
              
              // Дата документа
              AddCellToRow(asposeEpenseReport, newRow, expenseRow.ExpenseDate.Value.ToString("d"));
              
              // Номер документа
              if (expenseRow.ExpenseNumber != null)
                AddCellToRow(asposeEpenseReport, newRow, expenseRow.ExpenseNumber);
              else
                AddCellToRow(asposeEpenseReport, newRow, String.Empty);
              
              // Наименование документа (расхода)
              AddCellToRow(asposeEpenseReport, newRow, expenseRow.ExpenseDescription);

              // Сумма расхода
              AddCellToRow(asposeEpenseReport, newRow, expenseRow.ExpenseSum.Value.ToString("F", CultureInfo.InvariantCulture));
              
              // + ещё 4 ячейки, которые не заполняем
              for (var i = 0; i < 4; i++)
                AddCellToRow(asposeEpenseReport, newRow, String.Empty);
              
              // Вставляем строку в нужное место в таблице
              lastTable.InsertAfter(newRow, lastTable.Rows[lastTable.Rows.Count - 2]);
              rowNumber++;
            }
          }
          
          // Перенести тело документа Aspose в последнюю версию документа
          using (var docStream = new System.IO.MemoryStream())
          {
            asposeEpenseReport.Save(docStream, SaveFormat.Docx);
            newVersion.Body.Write(docStream);
          }
        }
        _obj.Save();
        
        // Сконвертировать в PDF
        Functions.Module.ConvertLastVersionToPDF(_obj);
      }
    }
    
    /// <summary>
    /// Добавить ячейку в строку таблицы расходов шаблона авансового отчета
    /// </summary>
    /// <param name="asposeDoc"> Документ Aspose.</param>
    /// <param name="row">Строка таблицы.</param>
    /// <param name="cellText">Содержимое ячейки.</param>
    public void AddCellToRow(Aspose.Words.Document asposeDoc, Aspose.Words.Tables.Row row, string cellText)
    {
      var newCell = new Aspose.Words.Tables.Cell(asposeDoc);
      
      var newParagraph = new Paragraph(asposeDoc);
      
      newParagraph.AppendChild(new Run(asposeDoc, cellText));
      
      // HACK Shestakov_AV При добавлении новых строк Таблицы, стили сбиваются. Пока сделал вот так.
      
      newParagraph.ParagraphFormat.Style.Font.Size = 8;
      newParagraph.ParagraphFormat.Alignment = ParagraphAlignment.Center;
      
      newCell.AppendChild(newParagraph);
      row.AppendChild(newCell);
    }
    
    /// <summary>
    /// Проверить, есть ли задачи на согласование в работе.
    /// </summary>
    /// <returns>True - задачи найдены, иначе false.</returns>
    [Remote(IsPure = true)]
    public bool CheckExpenseReportApprovalTasksByDocument()
    {
      var expenseReportTypeGuid = DirRX.ExpenseReports.Server.ExpenseReport.ClassTypeGuid;
      
      // Костыль, скопировано из стандартного слоя. Не работает поиск по группам вложений в LINQ.
      return ExpenseReportApprovalTasks.GetAll()
        .Where(t => t.Status == Sungero.Workflow.Task.Status.InProcess)
        .Where(t => t.AttachmentDetails.Any(a => a.AttachmentId == _obj.Id && a.AttachmentTypeGuid == expenseReportTypeGuid)).Any();
    }
    
    /// <summary>
    /// Получить командировку для авансового отчета.
    /// </summary>
    /// <returns>Командировка.</returns>
    [Public, Remote]
    public virtual DirRX.BusinessTrips.IBusinessTrip GetBusinessTrip()
    {
      return DirRX.BusinessTrips.BusinessTrips.GetAll(t => Equals(t.ExpenseReport, _obj)).FirstOrDefault();
    }
  }
}