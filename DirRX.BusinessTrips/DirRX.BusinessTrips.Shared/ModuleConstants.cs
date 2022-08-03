using System;
using Sungero.Core;

namespace DirRX.BusinessTrips.Constants
{
  public static class Module
  {

    /// <summary>
    /// Количество дней до отправки напоминаний о командировке.
    /// </summary>
    [Public]
    public const int ExpenseReportRemindMessageDelayDays = 3;

    [Public]
    public static class BusinessTripRoleGuids
    {
      /// <summary>
      /// Guid роли Бухгалтер ответственный за командировки.
      /// </summary>
      public static readonly Guid Accountant = Guid.Parse("14D8B007-FF85-4F21-8F1B-7051D286D578");
      
      /// <summary>
      /// Guid роли Ответственный за покупку билетов для командировок.
      /// </summary>
      public static readonly Guid TiketsResponsible = Guid.Parse("5380392D-6023-4BA9-AEDB-1F64BD4599D2");

      /// <summary>
      /// Guid роли Подписывающий приказ о командировке.
      /// </summary>
      public static readonly Guid OrderSigner = Guid.Parse("96B91CF4-C3EE-4AE0-80E9-470F26193A54");
      
    }

    /// <summary>
    /// Guid видов документов.
    /// </summary>
    [Public]
    public static class DocumentKind
    {
      /// <summary>
      /// Guid вида документа Приказ о направлении в командировку.
      /// </summary>
      public static readonly Guid BusinessTripOrderKind = Guid.Parse("14E0F8A5-07FA-45E4-86EE-667AA0087470");
      
      /// <summary>
      /// Guid вида документа Приказ об отзыве из командировки.
      /// </summary>
      public static readonly Guid BusinessTripRecallOrderKind = Guid.Parse("6E3A306C-0207-4202-B427-EC78D4914E1D");

      /// <summary>
      /// Guid вида документа Приказ об изменении командировки.
      /// </summary>
      public static readonly Guid BusinessTripChangeOrderKind = Guid.Parse("1BB04393-0D6B-4C6A-A552-8F760ABD1FB8");
      
      /// <summary>
      /// Guid вида документа Приказ об отмене командировки.
      /// </summary>
      public static readonly Guid BusinessTripCancelOrderKind = Guid.Parse("93AA260B-2FE1-4A24-A48D-042EB3595FEF");
      
      /// <summary>
      /// Guid вида документа Служебная записка по командировке.
      /// </summary>
      public static readonly Guid BusinessTripMemoKind = Guid.Parse("F5F1FBC1-84EE-4A97-BEDB-102C2D2725D9");
    }

    /// <summary>
    /// Наименования шаблонов документов.
    /// </summary>
    [Public]
    public static class TemplateNames
    {
      public const string OrderTemplateName = "Шаблон приказа о направлении в командировку";
      public const string RecallTripOrderTemplateName = "Шаблон приказа об отзыве из командировки";
      public const string CancelTripOrderTemplateName = "Шаблон приказа об отмене командировки";
      public const string ChangeTripOrderTemplateName = "Шаблон приказа об изменении командировки";
      public const string MemoTemplateName = "Шаблон служебной записки по командировке";
    }
    
    /// <summary>
    /// Ключи параметров Docflow.
    /// </summary>
    public static class DocflowParamKeys
    {
      /// <summary>
      /// Ключ параметра Тип расходов - Суточные.
      /// </summary>
      public const string PerDiemExpenseTypeIdKey = "PerDiemExpenseType";
      
      /// <summary>
      /// Ключ параметра Тип расходов - Проживание.
      /// </summary>
      public const string HotelExpenseTypeIdKey = "HotelExpenseType";
    }
    
    // Имя типа связи "Отменяет".
    [Public]
    public const string CancelRelationName = "Cancel";
  }
}