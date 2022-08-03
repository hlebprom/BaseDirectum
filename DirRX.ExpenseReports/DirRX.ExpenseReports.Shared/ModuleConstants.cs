using System;
using Sungero.Core;

namespace DirRX.ExpenseReports.Constants
{
  public static class Module
  {
    #region Константы для Smart захвата документов
    
    // Имя линии
    public const string LineName = "ExpenseReportsLine";
    
    // Наименования классов из классификатора Ario.
    public static class NewArioClassNames
    {
      public const string Receipt = "Чек";
      
      public const string AirTicket = "Авиабилет";
      
      public const string RailwayTicket = "Железнодорожный билет";
    }
    
    // Наименование правил для извлечения фактов Ario.
    public static class NewArioGrammarNames
    {
      public const string Default = "Default";
      
      public const string AirTicket = "AirTicket";
      
      public const string RailwayTicket = "RailwayTicket";
    }
    
    // Наименования полей для фактов Ario.
    public static class NewFieldNames
    {
      public static class DocumentAmount
      {
        public const string Amount = "Amount";
        
        public const string Currency = "Currency";
      }
      
      public static class Tiket
      {
        public const string DepartureDate = "DepartureDate";
        
        public const string Route = "Route";
      }
    }
    
    // Наименования фактов Ario.
    public static class NewFactNames
    {
      public const string DocumentAmount = "DocumentAmount";
      
      public const string ElectronicTicket = "ElectronicTicket";
    }
    
    #endregion
    
    // Код цвета подсветки превышения лимита.
    [Public]
    public const string LimitHighlightColorRed = "#FAC6B6";
    
    /// <summary>
    /// Перевод Мб в байты и наоборот 1024*1024.
    /// </summary>
    [Public]
    public const int ConvertMb = 1048576;
    
    /// <summary>
    /// Максимальное количество файлов в выгружаемом zip архиве в веб.
    /// </summary>
    [Public]
    public const int ExportedFilesCountMaxLimit = 5000;
    
    /// <summary>
    /// Максимальная сумма размеров файлов в выгружаемом zip архиве в веб.
    /// </summary>
    [Public]
    public const int ExportedFilesSizeMaxLimitMb = 450;
    
    /// <summary>
    /// Guid видов документов.
    /// </summary>
    [Public]
    public static class DocumentKindGuids
    {
      /// <summary>
      /// Guid вида документа Авансовый отчет.
      /// </summary>
      public static readonly Guid ExpenseReportKind = Guid.Parse("AAF1BD3F-A454-422B-94C0-6190652FE4F9");
      
      /// <summary>
      /// Guid вида документа Авиабилет.
      /// </summary>
      [Public]
      public static readonly Guid AviaKind = Guid.Parse("3D532E6F-7C80-4496-8CC9-A184CFCAA418");
      
      /// <summary>
      /// Guid вида документа Железнодорожный билет.
      /// </summary>
      [Public]
      public static readonly Guid RailwayTicketKind = Guid.Parse("9881E9F2-6BDF-4DE4-8709-5392D3C98515");
      
      /// <summary>
      /// Guid вида документа Кассовый чек.
      /// </summary>
      [Public]
      public static readonly Guid ReceiptKind = Guid.Parse("1028B59E-7049-43C6-97E6-4F289EB388A1");
      
      /// <summary>
      /// Guid вида документа Прочий подтверждающий документ.
      /// </summary>
      [Public]
      public static readonly Guid SupportingDocKind = Guid.Parse("786B0D1C-35F5-4F25-B45D-D144CC380CA3");
    }
    
    [Public]
    public static class ExpenseReportRoleGuids
    {
      /// <summary>
      /// Guid роли Бухгалтер, ответственный за авансовые отчеты.
      /// </summary>
      public static readonly Guid Accountant = Guid.Parse("6E1256EB-F851-4ACC-95FE-79293666BC7C");
      
    }
    
    /// <summary>
    /// Наименования шаблонов документов.
    /// </summary>
    [Public]
    public static class TemplateNames
    {
      public const string ExpenseReportTemplateName = "Шаблон авансового отчета";
    }
    
    // Имя типа связи "Прочие".
    [Public]
    public const string SimpleRelationName = "Simple relation";
    
    /// <summary>
    /// Ключи параметров Docflow.
    /// </summary>
    [Public]
    public static class DocflowParamKeys
    {
      /// <summary>
      /// Ключ параметра для почтового ящика для занесения подтв. документов.
      /// </summary>
      [Public]
      public const string SupportingDocsMailKey = "SupportingDocsMailKey";
      
      /// <summary>
      /// Ключ параметра режима работы с решением BTER.
      /// </summary>
      [Public]
      public const string ModeKey = "DirRX.BTERSolution.Mode";
    }
    
  }
}