using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.ExpenseReports.Structures.Module
{

  /// <summary>
  /// Информация для валидации подписания.
  /// </summary>
  partial class BeforeSign
  {
    public List<string> Errors { get; set; }
    
    public bool CanApprove { get; set; }
  }
  
  [Public]
  partial class ExportDialogSearch
  {
    public DateTime? From { get; set; }
    
    public DateTime? To { get; set; }
    
    public Sungero.Company.IBusinessUnit Unit { get; set; }
    
    public Sungero.Company.IEmployee Employee { get; set; }
  }
  
  partial class ExportResults
  {
    public string RootFolder { get; set; }
    
    public List<DirRX.ExpenseReports.Structures.Module.ExportedDocument> ExportedDocuments { get; set; }
    
    public List<DirRX.ExpenseReports.Structures.Module.ZipModel> ZipModels { get; set; }
  }
  
  partial class ExportedDocument
  {
    public int Id { get; set; }
    
    public bool IsFaulted { get; set; }
    
    public string Error { get; set; }
    
    // Папка самого документа всегда с пустым именем, это фактически корень общий для всех.
    public DirRX.ExpenseReports.Structures.Module.ExportedFolder Folder { get; set; }
    
    public string Name { get; set; }
  }

  partial class ExportedFolder
  {
    public string FolderName { get; set; }
  }
  
  partial class ZipModel
  {
    public int? DocumentId { get; set; }
    
    public int? VersionId { get; set; }
    
    public string FileName { get; set; }
    
    public List<string> FolderRelativePath { get; set; }
    
    public int? SignatureId { get; set; }
    
    public long Size { get; set; }
    
    public byte[] Body { get; set; }
  }
}