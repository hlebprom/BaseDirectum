using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Commons;
using Sungero.Content;
using Sungero.Company;
using Aspose.Words;
using Aspose.Pdf;
using Sungero.ClientExtensions;
using System.Xml;
using System.Net;
using System.IO;
using FieldNames = DirRX.ExpenseReports.Constants.Module.NewFieldNames;
using FactNames = DirRX.ExpenseReports.Constants.Module.NewFactNames;
using System.Globalization;

namespace DirRX.ExpenseReports.Server
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Получить действующие записи справочника Типы расходов.
    /// </summary>
    /// <returns>Список действующих типов расходов.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IExpenseType> GetActiveExpenseTypes()
    {
      return DirRX.ExpenseReports.ExpenseTypes.GetAll(e => e.Status == DirRX.ExpenseReports.ExpenseType.Status.Active);
    }
    
    #region Функции для работы с ролями.
    
    /// <summary>
    /// Получить исполнителя роли для нашей организации.
    /// Если такой не найден, то вернуть первого указанного в роли.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в нашей организации.</returns>
    [Public]
    public virtual IRecipient GetRoleRecipientForBusinessUnit(Guid roleGuid, IBusinessUnit businessUnit)
    {
      var recipientList = GetRoleRecipients(roleGuid, new List<Guid>());

      // Выбрать исполнителя для НОР, либо вернуть первого, указанного в роли.
      if (recipientList.Any())
        return recipientList.Where(l => Equals(Employees.As(l).Department.BusinessUnit, businessUnit)).FirstOrDefault() ?? recipientList.First();
      else
        return Recipients.Null;
    }
    
    /// <summary>
    /// Получить исполнителей роли.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <param name="processedRoles">Список guid'ов обработанных ролей.</param>
    /// <returns>Список исполнителей.</returns>
    [Public]
    public virtual List<IRecipient> GetRoleRecipients(Guid roleGuid, List<Guid> processedRoles)
    {
      var recipientList = new List<IRecipient>();
      if (!processedRoles.Contains(roleGuid))
      {
        processedRoles.Add(roleGuid);
        var role = GetRole(roleGuid);
        if (role != null)
        {
          var serviceUsersRole = GetRole(Sungero.Domain.Shared.SystemRoleSid.ServiceUsers);
          
          foreach (var recipient in role.RecipientLinks)
          {
            var member = recipient.Member;
            if (Users.Is(member))
            {
              // Добавить в список пользователей, которые не являются служебными
              if(!Users.As(member).IncludedIn(serviceUsersRole))
                recipientList.Add(member);
            }
            else if (Roles.Is(member))
              // Развернуть роль до конечных пользователей и добавить их в список.
              recipientList.AddRange(GetRoleRecipients(Roles.As(member).Sid.Value, processedRoles));
            else if (Departments.Is(member))
              // Добавить в список подразделения.
              recipientList.AddRange(GetDepartmentRecipients(Departments.As(member)));
          }
        }
        return recipientList.Where(l => l.Status == Sungero.CoreEntities.Recipient.Status.Active).Distinct().ToList();
      }
      else
        return recipientList;
    }
    
    /// <summary>
    /// Получить сотрудников подразделения.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <returns>Список сотрудников.</returns>
    [Public]
    public virtual List<IRecipient> GetDepartmentRecipients(IDepartment department)
    {
      var recipientList = new List<IRecipient>();
      recipientList.AddRange(department.RecipientLinks.Select(l => l.Member));
      var subDepartments = Departments.GetAll(l => Equals(l.HeadOffice, department));
      if (subDepartments.Any())
        foreach (var subDepartment in subDepartments)
          recipientList.AddRange(GetDepartmentRecipients(subDepartment));
      return recipientList;
    }
    
    #endregion
    
    #region Функции определения руководителя сотрудника
    
    /// <summary>
    /// Определить руководителя сотрудника с учетом головных подразделений.
    /// По иерархии подразделений найти руководителя для текущего сотрудника.
    /// Если у подразделения указан руководитель отличный от переданного сотрудника, то вернуть его,
    ///   иначе проверять руководителей головных подразделений.
    /// Если среди головных подразделений не найден руководитель отличный от переданного сотрудника,
    ///   то вернуть руководителя нашей организации.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Руководитель.</returns>
    [Remote(IsPure = true)]
    public virtual IEmployee GetManager(IDepartment department, IEmployee employee)
    {
      var manager = department.Manager;
      var headOffice = department.HeadOffice;
      if (manager == null || Equals(employee, manager))
        if (headOffice != null)
          manager = GetManager(headOffice, employee);
        else
          manager = department.BusinessUnit.CEO;
      return manager;
    }
    
    /// <summary>
    /// Получить руководителя сотрудника.
    /// Если в подразделении сотрудника руководитель не указан, то ищется ближайший руководитель по цепочке головных подразделений.
    /// Если не будет найден ни один руководитель в цепочке головных подразделений - веруть руководителя организации.
    /// Если руководитель не будет найден - вернуть null.
    /// Если переданный сотрудник является руководителем организации - вернуть его.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Руководитель.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IEmployee GetManager(IEmployee employee)
    {
      var department = employee.Department;
      return GetManager(department, employee);
    }
    
    #endregion
    
    #region Поиск настройки лимита по критериям.
    
    /// <summary>
    /// Получить настройку лимита.
    /// </summary>
    /// <param name="tripDestinationExpense">Структура с данными о месте назначения и типе расхода.</param>
    /// <returns>Настройка лимита.</returns>
    [Public, Remote(IsPure = true)]
    public virtual DirRX.ExpenseReports.ILimitSetting GetLimitSetting(IExpenseType expenseType, Sungero.Company.IEmployee employee, Sungero.Commons.ICity city)
    {
      var expenseTypeSettings = DirRX.ExpenseReports.LimitSettings.GetAll(s => s.Status == DirRX.ExpenseReports.LimitSetting.Status.Active && Equals(s.ExpenseType, expenseType));
      
      var employeeSettings = expenseTypeSettings.Where(s => Equals(s.Employee, employee));
      if (employeeSettings.Any())
        return GetDestinationSetting(employeeSettings, city);
      else
      {
        var jobTitleSettings = expenseTypeSettings.Where(s => s.Employee == null && Equals(s.Department, employee.Department) && Equals(s.JobTitle, employee.JobTitle));
        if (jobTitleSettings.Any())
          return GetDestinationSetting(jobTitleSettings, city);
        else
        {
          var departmentSettings = expenseTypeSettings.Where(s => s.JobTitle == null && s.Employee == null && Equals(s.Department, employee.Department));
          if (departmentSettings.Any())
            return GetDestinationSetting(departmentSettings, city);
          else
          {
            var destinationSettings = expenseTypeSettings.Where(s => s.Department == null && s.Employee == null);
            if (city != null)
            {
              var citySetting = GetCitySetting(destinationSettings, city);
              if (citySetting != null)
                return citySetting;
              else
              {
                var countrySetting = GetCountrySetting(destinationSettings, city.Country);
                if (countrySetting != null)
                  return countrySetting;
                else
                  return destinationSettings.Where(s => !s.Countries.Any() && !s.Cities.Any()).FirstOrDefault();
              }
            }
            else
              return destinationSettings.Where(s => !s.Countries.Any() && !s.Cities.Any()).FirstOrDefault();
          }
        }
      }
    }
    
    /// <summary>
    /// Выбрать из списка настройку лимита по месту расхода.
    /// </summary>
    /// <remarks>
    /// Если не выбрана настройка по месту расхода, будет возвращена настройка без детализации города\страны.
    /// </remarks>
    /// <param name="settings">Список настроек.</param>
    /// <param name="city">Город.</param>
    /// <returns>Настройка лимита.</returns>
    public virtual DirRX.ExpenseReports.ILimitSetting GetDestinationSetting(IQueryable<DirRX.ExpenseReports.ILimitSetting> settings, Sungero.Commons.ICity city)
    {
      if (city != null)
      {
        var citySetting = GetCitySetting(settings, city);
        if (citySetting != null)
          return citySetting;
        else
        {
          var countrySetting = GetCountrySetting(settings, city.Country);
          if (countrySetting != null)
            return countrySetting;
          else
            return settings.Where(s => !s.Countries.Any() && !s.Cities.Any()).FirstOrDefault();
        }
      }
      else
        return settings.Where(s => !s.Countries.Any() && !s.Cities.Any()).FirstOrDefault();
    }

    /// <summary>
    /// Выбрать из списка настройку лимита по стране.
    /// </summary>
    /// <param name="settings">Список настроек.</param>
    /// <param name="country">Страна.</param>
    /// <returns>Настройка лимита.</returns>
    public virtual DirRX.ExpenseReports.ILimitSetting GetCountrySetting(IQueryable<DirRX.ExpenseReports.ILimitSetting> settings, Sungero.Commons.ICountry country)
    {
      var countrySettings = settings.Where(s => s.Countries.Any());
      foreach (var countrySetting in countrySettings)
      {
        if (countrySetting.Countries.Select(c => c.Country).Contains(country))
          return countrySetting;
      }
      return DirRX.ExpenseReports.LimitSettings.Null;
    }
    
    /// <summary>
    /// Выбрать из списка настройку лимита по городу.
    /// </summary>
    /// <param name="settings">Список настроек.</param>
    /// <param name="city">Город.</param>
    /// <returns>Настройка лимита.</returns>
    public virtual DirRX.ExpenseReports.ILimitSetting GetCitySetting(IQueryable<DirRX.ExpenseReports.ILimitSetting> settings, Sungero.Commons.ICity city)
    {
      var citySettings = settings.Where(s => s.Cities.Any());
      foreach (var citySetting in citySettings)
      {
        if (citySetting.Cities.Select(c => c.City).Contains(city))
          return citySetting;
      }
      return DirRX.ExpenseReports.LimitSettings.Null;
    }
    
    #endregion
    
    #region Курсы валют
    
    /// <summary>
    /// Загрузить курсы валют с сайта ЦБ РФ.
    /// </summary>
    [Remote, Public]
    public virtual void DownloadRatesFromSite()
    {
      // Получить список валют с сайта ЦБ РФ
      Logger.Debug("CurrencyRates. Currencies start download from http://www.cbr.ru/scripts/XML_val.asp?d=0.");
      var сurrenciesDataFromSite = GetRequestResult("http://www.cbr.ru/scripts/XML_val.asp?d=0");
      Logger.Debug("CurrencyRates. Currencies download complete.");

      // Если культура тенанта - русская, то наименование валюты взять из узла Name, для английской культуры использовать узел EngName.
      string currencyNodeName = "Name";
      if (TenantInfo.Culture.Name != "ru-RU")
        currencyNodeName = "EngName";
      
      var CurrenciesXML = new XmlDocument();
      CurrenciesXML.LoadXml(сurrenciesDataFromSite);
      
      var currencyNodes = CurrenciesXML.SelectNodes("/Valuta/Item");
      
      // Загрузить курсы для всех действующих валют, которые есть в справочнике "Валюты"
      foreach (var currency in Sungero.Commons.Currencies.GetAll(c => c.Status == Sungero.Commons.Currency.Status.Active))
      {
        var currencyName = currency.Name;
        foreach (XmlNode currencyNode in currencyNodes)
        {
          if (currencyNode[currencyNodeName].InnerText == currencyName)
          {
            var currencyCode = currencyNode.Attributes["ID"].Value;
            
            // Вычислить начальную дату для загрузки курса по валюте.
            DateTime startDate;
            var lastRate = CurrencyRates.GetAll(r => Equals(r.Currency, currency)).OrderByDescending(r => r.Date).FirstOrDefault();
            if (lastRate != null)
              // Курсы будут получены, начиная со следующего за последней датой дня.
              startDate = lastRate.Date.Value.AddDays(1);
            else
              // Если курсы для валюты ещё не загружались, то загрузить за полследний год.
              startDate = Sungero.Core.Calendar.Today.AddYears(-1);
            
            if (startDate < Sungero.Core.Calendar.Today)
            {
              // Получить курсы валюты с сайта ЦБ РФ
              string currencyRatesData = null;
              try
              {
                Logger.DebugFormat("CurrencyRates. Rates for currency {0} download start.", currency);
                var url = String.Format("http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={0:dd/MM/yyyy}&date_req2={1:dd/MM/yyyy}&VAL_NM_RQ={2}", startDate, Sungero.Core.Calendar.Today, currencyCode);
                Logger.DebugFormat("CurrencyRates. Rates for currency request URL: {0}", url);
                currencyRatesData = GetRequestResult(url);
                Logger.DebugFormat("CurrencyRates. Rates for currency {0} download complete.", currency);
              }
              catch (WebException e)
              {
                Logger.ErrorFormat("CurrencyRates. Rates for currency {0} download error: {1}", currency, e.Message);
              }
              
              if (currencyRatesData != null)
              {
                var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                
                var ratesXML = new XmlDocument();
                ratesXML.LoadXml(currencyRatesData);
                
                var ratesNodes = ratesXML.SelectNodes("/ValCurs/Record");
                
                foreach (XmlNode rateNode in ratesNodes)
                {
                  var nominal = Int32.Parse(rateNode["Nominal"].InnerText);
                  
                  var rateValue = Double.Parse(rateNode["Value"].InnerText.Replace(",", separator));
                  
                  var newRate = CurrencyRates.Create();
                  newRate.IsAuto = true;
                  newRate.Date = DateTime.ParseExact(rateNode.Attributes["Date"].InnerText, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                  newRate.Currency = currency;
                  newRate.Rate = rateValue / nominal;
                  newRate.Save();
                }
              }
            }

            // После загрузки курсов перейти к следующей валюте.
            break;
          }
        }
      }
      Logger.DebugFormat("CurrencyRates. Currency rates download complete.");
    }
    
    /// <summary>
    /// Получить курс на указанную дату.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="date">Дата.</param>
    /// <returns>Курс.</returns>
    /// <remarks>Если на указанную дату нет курса, то вернуть курс на ближайшую предыдущую.
    /// Если до указанной даты курсов не было, вернуть ближайший из курсов на следующие дни.
    /// Если занесенных в систему курсов нет, то вернуть null.</remarks>
    [Remote, Public]
    public virtual double? GetCurrencyRateForDate(Sungero.Commons.ICurrency currency, DateTime date)
    {
      var rateForDate = CurrencyRates.GetAll(r => Equals(r.Currency, currency) && r.Date <= date).OrderByDescending(r => r.Date).FirstOrDefault();
      if (rateForDate == null)
      {
        rateForDate = CurrencyRates.GetAll(r => Equals(r.Currency, currency) && r.Date > date).OrderBy(r => r.Date).FirstOrDefault();
        if (rateForDate == null)
          return null;
      }
      
      return rateForDate.Rate;
    }

    /// <summary>
    /// Получить результат выполнения GET запроса.
    /// </summary>
    /// <param name="url">Url запроса.</param>
    /// <returns>Результат выполнения.</returns>
    private string GetRequestResult(string url)
    {
      var request = HttpWebRequest.Create(url);
      var resp = (HttpWebResponse)request.GetResponse();
      
      string result;
      using (var stream = resp.GetResponseStream())
        using (var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1251")))
          result = reader.ReadToEnd();

      return result;
    }
    
    /// <summary>
    /// Получить валюту "Российский рубль".
    /// </summary>
    [Remote, Public]
    public virtual Sungero.Commons.ICurrency GetCurrencyRUB()
    {
      var rub = Sungero.Commons.Currencies.GetAll(cr => cr.AlphaCode == "RUB").FirstOrDefault();
      if (rub == null)
        Logger.Debug("No currency RUB in Currencies databook.");
      return rub;
    }
    
    #endregion
    
    /// <summary>
    /// Получить шаблон для авансового отчета.
    /// </summary>
    [Public]
    public virtual IElectronicDocumentTemplate GetTemplateForExpenseReport()
    {
      return Sungero.Content.ElectronicDocumentTemplates
        .GetAll(n => n.Name == DirRX.ExpenseReports.Constants.Module.TemplateNames.ExpenseReportTemplateName).FirstOrDefault();
    }
    
    /// <summary>
    /// Создать авансовый отчет.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="purpose">Назначение аванса.</param>
    /// <param name="gettedMoney">Итого получено.</param>
    /// <returns>Созданный авансовый отчет.</returns>
    [Public, Remote]
    public virtual IExpenseReport CreateExpenseReport(IEmployee employee, string purpose, double gettedMoney)
    {
      var document = ExpenseReports.Create();
      document.DocumentKind = PublicFunctions.Module.Remote.GetDocumentKind(DirRX.ExpenseReports.Constants.Module.DocumentKindGuids.ExpenseReportKind);
      document.Employee = employee;
      document.Purpose = purpose;
      document.GettedMoney = gettedMoney;
      
      // Выдать права сотруднику.
      document.AccessRights.Grant(employee, DefaultAccessRightsTypes.FullAccess);
      
      document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Построить модель состояния для вывода простого текста.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Public]
    public virtual StateView GetTextState(string text)
    {
      var stateView = Sungero.Core.StateView.Create();
      var block = stateView.AddBlock();
      var content = block.AddContent();
      content.AddLabel(text);
      block.ShowBorder = false;
      return stateView;
    }

    /// <summary>
    /// Проверить, что включен режим отладки.
    /// </summary>
    /// <returns>True, если включен режим отладки, инае False.</returns>
    [Public, Remote]
    public bool IsDebugEnabled()
    {
      return Resources.DebugModeName == GetDocflowParamValue(DirRX.ExpenseReports.PublicConstants.Module.DocflowParamKeys.ModeKey);
    }
    
    #region Функции для захвата и обработки документов
    
    /// <summary>
    /// Обработать пакет подтверждающих документов с почты.
    /// </summary>
    /// <param name="blobPackage">Пакет документов из DCS.</param>
    [Remote(IsPure = true), Public]
    public virtual void ProcessCapturedPackage(Sungero.SmartProcessing.IBlobPackage blobPackage)
    {
      var arioPackage = Sungero.SmartProcessing.PublicFunctions.Module.UnpackArioPackage(blobPackage);
      
      var documentPackage = BuildDocumentPackage(blobPackage, arioPackage);
      
      Sungero.SmartProcessing.PublicFunctions.Module.OrderAndLinkDocumentPackage(documentPackage);
      
      // Прикрепить документы к авансовому отчету
      var docs = new List<Sungero.Docflow.IOfficialDocument>();
      foreach (var documentInfo in documentPackage.DocumentInfos)
      {
        docs.Add(documentInfo.Document);
      }
      AddDocsToExpenseReport(docs, documentPackage.Responsible);

      Sungero.SmartProcessing.PublicFunctions.Module.FinalizeProcessing(blobPackage);
    }
    
    /// <summary>
    /// Сформировать пакет документов.
    /// </summary>
    /// <param name="blobPackage">Пакет документов из DCS.</param>
    /// <param name="arioPackage">Пакет результатов обработки документов в Ario.</param>
    /// <returns>Пакет созданных документов.</returns>
    [Public]
    public virtual Sungero.SmartProcessing.Structures.Module.IDocumentPackage BuildDocumentPackage(Sungero.SmartProcessing.IBlobPackage blobPackage,
                                                                                                   Sungero.SmartProcessing.Structures.Module.IArioPackage arioPackage)
    {
      var documentPackage = PrepareDocumentPackage(blobPackage, arioPackage);
      
      foreach (var documentInfo in documentPackage.DocumentInfos)
      {
        var document = CreateDocument(documentInfo, documentPackage);
        
        Sungero.SmartProcessing.PublicFunctions.Module.CreateVersion(document, documentInfo);
        
        if (!documentInfo.FailedCreateVersion)
        {
          Sungero.SmartProcessing.PublicFunctions.Module.FillVerificationState(document);
        }
        
        Sungero.SmartProcessing.PublicFunctions.Module.SaveDocument(document, documentInfo);
      }
      
      return documentPackage;
    }
    
    /// <summary>
    /// Создать незаполненный пакет документов.
    /// </summary>
    /// <param name="blobPackage">Пакет документов из DCS.</param>
    /// <param name="arioPackage">Пакет результатов обработки документов в Ario.</param>
    /// <returns>Заготовка пакета документов.</returns>
    [Public]
    public virtual Sungero.SmartProcessing.Structures.Module.IDocumentPackage PrepareDocumentPackage(Sungero.SmartProcessing.IBlobPackage blobPackage,
                                                                                                     Sungero.SmartProcessing.Structures.Module.IArioPackage arioPackage)
    {
      var documentPackage = Sungero.SmartProcessing.Structures.Module.DocumentPackage.Create();
      
      // Найти ответственного.
      var responsible = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetEmpoyeeByEmail(blobPackage.FromAddress);
      
      documentPackage.Responsible = responsible;
      
      var documentInfos = new List<Sungero.SmartProcessing.Structures.Module.IDocumentInfo>();

      var arioSettings = Sungero.Docflow.PublicFunctions.SmartProcessingSetting.GetSettings();
      
      foreach (var arioDocument in arioPackage.Documents)
      {
        // TODO Smart: Логичнее заполнение этого свойства делать в UnpackArioPackage,
        // Но тогда в тесте придется дублировать UnpackArioPackage, а она большая. Подумать, как лучше.
        if (arioDocument.IsProcessedByArio)
        {
          
          using (var bodyFromArio = Sungero.Docflow.PublicFunctions.SmartProcessingSetting.GetDocumentBody(arioSettings, arioDocument.BodyGuid))
          {
            var bufferLen = (int)bodyFromArio.Length;
            var buffer = new byte[bufferLen];
            bodyFromArio.Read(buffer, 0, bufferLen);
            arioDocument.BodyFromArio = buffer;
          }
        }
        else
        {
          // Если документ не ходил в Ario, то заполним свойство пустым массивом байт.
          // Чтобы не было дальнейших проблем при работе с этим свойством.
          arioDocument.BodyFromArio = new byte[0];
        }
        var documentInfo = Sungero.SmartProcessing.Structures.Module.DocumentInfo.Create();
        documentInfo.ArioDocument = arioDocument;
        documentInfo.IsRecognized = arioDocument.IsRecognized;
        documentInfos.Add(documentInfo);
      }

      documentPackage.DocumentInfos = documentInfos;
      documentPackage.BlobPackage = blobPackage;

      return documentPackage;
    }
    
    /// <summary>
    /// Создать документ.
    /// </summary>
    /// <param name="documentInfo">Информация о документе.</param>
    /// <param name="documentPackage">Пакет документов.</param>
    /// <returns>Созданный документ.</returns>
    [Public]
    public virtual IOfficialDocument CreateDocument(Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo, 
                                                    Sungero.SmartProcessing.Structures.Module.IDocumentPackage documentPackage)
    {
      var arioDocument = documentInfo.ArioDocument;
      var document = OfficialDocuments.Null;
      if (documentInfo.IsRecognized && arioDocument.IsProcessedByArio)
      {
        document = Sungero.SmartProcessing.PublicFunctions.Module.CreateDocumentByFacts(arioDocument.RecognitionInfo.RecognizedClass, 
                                                                                        documentInfo, 
                                                                                        documentPackage.Responsible);
      }
      else
        document = CreateSimpleSupportingDocument(documentInfo, documentPackage.Responsible);
      
      documentInfo.Document = document;

      // Выдать полные права ответственному
      document.AccessRights.Grant(documentPackage.Responsible, DefaultAccessRightsTypes.FullAccess);
      
      return document;
    }
    
    /// <summary>
    /// Создать подтверждающий документ - ЖД билет.
    /// </summary>
    /// <param name="documentInfo">Информация о документе.</param>
    /// <param name="responsible">Ответственный за верификацию.</param>
    /// <returns>Подтверждающий документ.</returns>
    [Public]
    public virtual IOfficialDocument CreateRailwayTicket(Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo,
                                                         IEmployee responsible)
    {
      // Подтверждающий документ.
      var document = SupportingDocuments.As(CreateSimpleSupportingDocument(documentInfo, responsible));
      document.DocumentKind = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(DirRX.ExpenseReports.PublicConstants.Module.DocumentKindGuids.RailwayTicketKind);
      
      FillDataForTicket(document, documentInfo);

      return document;
    }
    
    /// <summary>
    /// Создать подтверждающий документ - Авиа билет.
    /// </summary>
    /// <param name="documentInfo">Информация о документе.</param>
    /// <param name="responsible">Ответственный за верификацию.</param>
    /// <returns>Подтверждающий документ.</returns>
    [Public]
    public virtual IOfficialDocument CreateAirTicket(Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo,
                                                     IEmployee responsible)
    {
      // Подтверждающий документ.
      var document = SupportingDocuments.As(CreateSimpleSupportingDocument(documentInfo, responsible));
      document.DocumentKind = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(DirRX.ExpenseReports.PublicConstants.Module.DocumentKindGuids.AviaKind);

      FillDataForTicket(document, documentInfo);

      return document;
    }
    
    /// <summary>
    /// Создать подтверждающий документ - Чек.
    /// </summary>
    /// <param name="documentInfo">Информация о документе.</param>
    /// <param name="responsible">Ответственный за верификацию.</param>
    /// <returns>Подтверждающий документ.</returns>
    [Public]
    public virtual IOfficialDocument CreateReceipt(Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo,
                                                   IEmployee responsible)
    {
      // Подтверждающий документ.
      var document = SupportingDocuments.As(CreateSimpleSupportingDocument(documentInfo, responsible));
      document.DocumentKind = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(DirRX.ExpenseReports.PublicConstants.Module.DocumentKindGuids.ReceiptKind);
      
      FillDataForReceipt(document, documentInfo);

      return document;
    }
    
    /// <summary>
    /// Создать прочий подтверждающий документ.
    /// </summary>
    /// <param name="documentInfo">Информация о документе.</param>
    /// <param name="responsible">Ответственный за верификацию.</param>
    /// <returns>Подтверждающий документ.</returns>
    [Public]
    public virtual IOfficialDocument CreateSimpleSupportingDocument(Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo,
                                                                    IEmployee responsible)
    {
      // Подтверждающий документ.
      var document = SupportingDocuments.Create();
      document.DocumentKind = DirRX.ExpenseReports.PublicFunctions.Module.Remote.GetDocumentKind(DirRX.ExpenseReports.PublicConstants.Module.DocumentKindGuids.SupportingDocKind);
      
      // Заполнить основные свойства
      document.Employee = responsible;
      document.Author = responsible;
      
      // Содержание = исходное имя файла
      document.Subject = documentInfo.ArioDocument.OriginalBlob.OriginalFileName;
      
      // Дата расхода
      document.Date = Sungero.Core.Calendar.Today;

      return document;
    }

    /// <summary>
    /// Заполнить свойства Авиа/ЖД билета по результатам извлечения фактов в Ario.
    /// </summary>
    /// <param name="document">Подтверждающий документ.</param>
    /// <param name="documentInfo">Информация о документе.</param>
    /// <param name="responsible">Сотрудник, ответственный за обработку захваченных документов.</param>
    [Public]
    public virtual void FillDataForTicket(ISupportingDocument document,
                                          Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo)
    {
      var props = document.Info.Properties;
      var arioDocument = documentInfo.ArioDocument;
      
      // Сумма
      var amountFact = Sungero.Commons.PublicFunctions.Module.GetOrderedFacts(arioDocument.Facts,
                                                                              FactNames.DocumentAmount,
                                                                              FieldNames.DocumentAmount.Amount)
        .FirstOrDefault();
      
      if (amountFact != null)
      {
        document.Sum = Sungero.Commons.PublicFunctions.Module.GetFieldNumericalValue(amountFact,
                                                                                     FieldNames.DocumentAmount.Amount);
        Sungero.Commons.PublicFunctions.EntityRecognitionInfo.LinkFactAndProperty(arioDocument.RecognitionInfo,
                                                                                  amountFact,
                                                                                  FieldNames.DocumentAmount.Amount,
                                                                                  props.Sum.Name,
                                                                                  document.Sum);
      }
      
      // Валюта
      var сurrencyFact = Sungero.Commons.PublicFunctions.Module.GetOrderedFacts(arioDocument.Facts,
                                                                                FactNames.DocumentAmount,
                                                                                FieldNames.DocumentAmount.Currency)
        .FirstOrDefault();
      
      if (сurrencyFact != null)
      {
        // Ario извлекает Код валюты (643, 840...)
        var сurrencyCode = Sungero.Commons.PublicFunctions.Module.GetFieldNumericalValue(сurrencyFact,
                                                                                         FieldNames.DocumentAmount.Currency);
        if (сurrencyCode != null)
        {
          var currency = Sungero.Commons.Currencies.GetAll(c => c.NumericCode == сurrencyCode.ToString()).FirstOrDefault();
          if (currency != null)
          {
            document.Currency = currency;
            Sungero.Commons.PublicFunctions.EntityRecognitionInfo.LinkFactAndProperty(arioDocument.RecognitionInfo,
                                                                                      сurrencyFact,
                                                                                      FieldNames.DocumentAmount.Currency,
                                                                                      props.Currency.Name,
                                                                                      document.Currency);
          }
        }
      }
      
      // Дата расхода
      var dateFact = Sungero.Commons.PublicFunctions.Module.GetOrderedFacts(arioDocument.Facts,
                                                                            FactNames.ElectronicTicket,
                                                                            FieldNames.Tiket.DepartureDate)
        .FirstOrDefault();

      if (dateFact != null)
      {
        
        document.Date = Sungero.Commons.PublicFunctions.Module.GetFieldDateTimeValue(dateFact,
                                                                                     FieldNames.Tiket.DepartureDate);
        Sungero.Commons.PublicFunctions.EntityRecognitionInfo.LinkFactAndProperty(arioDocument.RecognitionInfo,
                                                                                  dateFact,
                                                                                  FieldNames.Tiket.DepartureDate,
                                                                                  props.Date.Name,
                                                                                  document.Date);
      }
      else
        document.Date = Sungero.Core.Calendar.Today;
      
      // Описание расхода
      var routeFact = Sungero.Commons.PublicFunctions.Module.GetOrderedFacts(arioDocument.Facts,
                                                                             FactNames.ElectronicTicket,
                                                                             FieldNames.Tiket.Route)
        .FirstOrDefault();

      if (routeFact != null)
      {
        document.Subject = Sungero.Commons.PublicFunctions.Module.GetFieldValue(routeFact,
                                                                                FieldNames.Tiket.Route);
        Sungero.Commons.PublicFunctions.EntityRecognitionInfo.LinkFactAndProperty(arioDocument.RecognitionInfo,
                                                                                  routeFact,
                                                                                  FieldNames.Tiket.Route,
                                                                                  props.Subject.Name,
                                                                                  document.Subject);
      }
    }
    
    /// <summary>
    /// Заполнить свойства чека по результатам извлечения информации из QR.
    /// </summary>
    /// <param name="document">Подтверждающий документ.</param>
    /// <param name="recognitionResult">Результат классификации в Ario.</param>
    [Public]
    public virtual void FillDataForReceipt(ISupportingDocument document,
                                           Sungero.SmartProcessing.Structures.Module.IDocumentInfo documentInfo)
    {
      using (var body = new MemoryStream(documentInfo.ArioDocument.BodyFromArio))
      {
        var docBarcodes = ExtractQR(body);
        if (docBarcodes.Any())
        {
          // Строка из QR чека, в идеале, имеет вид "t=20190503T0922&s=213.00&fn=9284000100000099&i=145000&fp=0000306862&n=1"
          // где t=20190503T0922 - Дата, s=213.00 - Сумма
          var parts = docBarcodes.First().Split('&');
          
          // Детальные проверки строки нужны, т.к. в QR, в общем случае, может быть зашифровано всё, что угодно, например, URL
          // Проверить, что в строке есть подстроки, разделенные "&", минимум 4 штуки.
          if (parts.Count() > 4)
          {
            // Получить дату
            if (parts[0].Split('=')[0] == "t")
            {
              var timePart = parts[0].Split('=')[1].Split('T')[0]; // "t=20190503T0922" -> "20190503T0922" -> 20190503
              var timeValue = new DateTime();
              if (DateTime.TryParseExact(timePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out timeValue))
                document.Date = timeValue;
            }
            
            // Получить Сумму
            if ( parts[1].Split('=')[0] == "s")
            {
              var field = parts[1].Split('=')[1];
              if (!string.IsNullOrWhiteSpace(field))
              {
                double sum;
                double.TryParse(field, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out sum);
                document.Sum = sum;
              }
            }
          }
        }
        
      }
      // Если дату извлечь не удалось, заполнить текущей
      if (!document.Date.HasValue)
        document.Date = Sungero.Core.Calendar.Today;
    }
    
    /// <summary>
    /// Получить сотрудника по email.
    /// </summary>
    /// <param name="email">E-mail сотрудника.</param>
    /// <returns> Сотрудник.</returns>
    [Remote, Public]
    public IEmployee GetEmpoyeeByEmail(string email)
    {
      var employee = Employees.GetAll(e => e.Email.ToLower() == email.ToLower()).FirstOrDefault();
      if (employee == null)
        throw new ApplicationException(Resources.NoEmployeeByEmailFormat(email));
      else
        Logger.DebugFormat("Captured Package Process. Employee founded '{0}'", employee.Name);
      
      return employee;
    }
    
    /// <summary>
    /// Добавить документы к последнему авансовому отчету на оформлении сотрудника.
    /// </summary>
    /// <param name="documents">Результат создания документов.</param>
    /// <param name="responsible">Сотрудник, отправивший подтверждающие документы.</param>
    [Remote, Public]
    public virtual void AddDocsToExpenseReport(List<Sungero.Docflow.IOfficialDocument> documents, IEmployee responsible)
    {
      var expenseReport = GetLastDraftExpenseReportByEmployee(responsible);
      
      if (expenseReport != null)
      {
        Logger.DebugFormat("Captured Package Process. Expense report founded '{0}'", expenseReport.Name);
        // Документы получится привязать, если авансовый отчет не заблокирован
        var lockInfo = Locks.GetLockInfo(expenseReport);
        if (!lockInfo.IsLocked && Locks.TryLock(expenseReport))
        {
          var expenses = expenseReport.Expenses;
          foreach (var doc in documents)
          {
            if (DirRX.ExpenseReports.SupportingDocuments.Is(doc))
            {
              var supportingDoc = DirRX.ExpenseReports.SupportingDocuments.As(doc);
              var newExpense = expenses.AddNew();
              newExpense.SupportingDoc = supportingDoc;
              
              // Если в текущей таблице расходов уже была строка с такими данными, то Сумму не заполнять,
              // чтобы документ все-таки был виден, но не было дубля суммы и в шаблон не попала лишняя строка
              if (expenses.Where(e => e.ExpenseDescription == supportingDoc.Subject &&
                                 e.ExpenseDate == supportingDoc.Date &&
                                 e.ExpenseSum == supportingDoc.Sum &&
                                 !Equals(e, newExpense)).Any())
                newExpense.ExpenseSum = null;
            }
          }
          expenseReport.Save();
          Locks.Unlock(expenseReport);
        }
        else
        {
          Logger.ErrorFormat("Captured Package Process. Expense report is locked. Id='{0}' ", expenseReport.Id);
          
          // Отправить уведомление пользователю о том, что не получилось заполнить расходы из-за блокировки
          documents.Add(expenseReport);
          CreateAddDocsToExpenseReportNotice(Resources.ExpenseReportLockedNoticeSubjectFormat(expenseReport.RegistrationNumber,
                                                                                              expenseReport.RegistrationDate.Value.ToString("d"),
                                                                                              expenseReport.Purpose),
                                             Resources.ExpenseReportLockedNoticeText,
                                             responsible,
                                             documents);
        }
      }
      else
      {
        Logger.ErrorFormat("Captured Package Process. Expense report for '{0}' not found", responsible.Name);
        
        // Отправить уведомление пользователю о том, что занесены новые подтверждающие документы
        CreateAddDocsToExpenseReportNotice(Resources.ExpenseReportNotFoundNoticeSubject,
                                           Resources.ExpenseReportNotFoundNoticeText,
                                           responsible,
                                           documents);
      }
    }
    
    /// <summary>
    /// Найти последний авансовый отчет сотрудника на оформлении.
    /// </summary>
    /// <param>Сотрудник.</param>
    /// <returns>Авансовый отчет.</returns>
    [Public]
    public virtual DirRX.ExpenseReports.IExpenseReport GetLastDraftExpenseReportByEmployee(Sungero.Company.IEmployee employee)
    {
      return DirRX.ExpenseReports.ExpenseReports.GetAll(r => r.ExpenseReportStatus == DirRX.ExpenseReports.ExpenseReport.ExpenseReportStatus.Start &&
                                                        Equals(r.Employee, employee))
        .OrderByDescending(e => e.Id)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Создать уведомление о занесении подтверждающих документов.
    /// </summary>
    /// <param name="subject">Тема уведомления.</param>
    /// <param name="text">Текст уведомления.</param>
    /// <param name="responsible">Исполнитель.</param>
    /// <param name="documents">Документы для вложения</param>
    [Public]
    public virtual void CreateAddDocsToExpenseReportNotice(string subject, string text, IEmployee responsible, List<Sungero.Docflow.IOfficialDocument> documents)
    {
      var notification = Sungero.Workflow.SimpleTasks.Create();
      foreach (var doc in documents)
      {
        notification.Attachments.Add(doc);
      }
      notification.Subject = subject;
      notification.ActiveText = text;
      var notificationRoute = notification.RouteSteps.AddNew();
      notificationRoute.AssignmentType = Sungero.Workflow.SimpleTaskRouteSteps.AssignmentType.Notice;
      notificationRoute.Performer = responsible;
      notification.NeedsReview = false;
      notification.Start();
    }
    
    /// <summary>
    /// Извлечь QR из pdf документа.
    /// </summary>
    /// <param name="inputStream">Pdf документ.</param>
    /// <returns>Список извлеченных штрихкодов</returns>
    /// <remarks>Извлекает штрихкоды в формате QR только с первой страницы документа.</remarks>
    [Public]
    public virtual List<string> ExtractQR(System.IO.Stream inputStream)
    {
      var docBarcodes = new List<string>();
      try
      {
        var barcodeReader = new Sungero.AsposeExtensions.BarcodeReader();
        docBarcodes = barcodeReader.Extract(inputStream, Aspose.BarCode.BarCodeRecognition.DecodeType.QR);
      }
      catch (Sungero.AsposeExtensions.BarcodeReaderException e)
      {
        Logger.Error(e.Message);
      }
      return docBarcodes;
    }
    
    #endregion
    
    /// <summary>
    /// Создать задачу на согласование заявки на аванс.
    /// </summary>
    /// <returns>Задача на согласование заявки на аванс.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public virtual IExpenseRequestApprovalTask CreateExpenseRequestTask()
    {
      return ExpenseRequestApprovalTasks.Create();
    }
    
    /// <summary>
    /// Добавить в таблицу параметров Docflow значение.
    /// </summary>
    /// <param name="key">Ключ параметра.</param>
    /// <param name="val">Значение.</param>
    [Remote(IsPure = true), Public]
    public static void AddDocflowParam(string key, string val)
    {
      Sungero.Docflow.PublicFunctions.Module.InsertOrUpdateDocflowParam(key, val);
    }

    /// <summary>
    /// Получить значение параметра Docflow.
    /// </summary>
    /// <param name="key">Ключ параметра.</param>
    /// <returns>Значение.</returns>
    [Remote, Public]
    public string GetDocflowParamValue(string key)
    {
      var command = string.Format(Sungero.Docflow.Queries.Module.SelectDocflowParamsValue, key);
      var commandExecutionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
      if (!(commandExecutionResult is DBNull) && commandExecutionResult != null)
        return commandExecutionResult.ToString();
      return string.Empty;
    }
    
    /// <summary>
    /// Создать объект с типом "Документ Aspose.Pdf".
    /// </summary>
    /// <param name="sungeroDocumentVersion">Версия документа из системы.</param>
    /// <returns>Документ Aspose.Pdf.</returns>
    public Aspose.Pdf.Document CreateAsposePdfDocument(Sungero.Content.IElectronicDocumentVersions sungeroDocumentVersion)
    {
      Aspose.Pdf.Document document;
      using (var memory = new System.IO.MemoryStream())
      {
        using (Sungero.Domain.Shared.HistoryLocker.WithoutWriteHistory(sungeroDocumentVersion.ElectronicDocument))
        {
          sungeroDocumentVersion.Body.Read().CopyTo(memory);
        }
        document = new Aspose.Pdf.Document(memory);
      }
      return document;
    }
    
    /// <summary>
    /// Создать объект с типом "Документ Aspose.Words".
    /// </summary>
    /// <param name="sungeroDocumentVersion">Версия документа из системы.</param>
    /// <returns>Документ Aspose.Words.</returns>
    public Aspose.Words.Document CreateAsposeWordDocument(Sungero.Content.IElectronicDocumentVersions sungeroDocumentVersion)
    {
      Aspose.Words.Document document;
      using (var memory = new System.IO.MemoryStream())
      {
        using (Sungero.Domain.Shared.HistoryLocker.WithoutWriteHistory(sungeroDocumentVersion.ElectronicDocument))
        {
          sungeroDocumentVersion.Body.Read().CopyTo(memory);
        }
        document = new Aspose.Words.Document(memory);
      }
      document.AutomaticallyUpdateStyles = false;
      document.RemoveMacros();
      return document;
    }
    
    /// <summary>
    /// Получить все мои авансовые отчеты.
    /// </summary>
    /// <returns>Список всех моих авансовых отчетов.</returns>
    [Remote(IsPure = true), Public]
    public virtual IQueryable<IExpenseReport> GetMyExpenseReports()
    {
      return ExpenseReports.GetAll().Where(t => t.Employee.Equals(Employees.Current));
    }
    
    /// <summary>
    /// Создать задачу на согласование авансового отчета
    /// </summary>
    /// <returns>Задача на согласование авансового отчета</returns>
    [Remote(PackResultEntityEagerly = true), Public]
    public virtual IExpenseReportApprovalTask CreateExpenseReportApprovalTask(IExpenseReport document)
    {
      var task = ExpenseReportApprovalTasks.Create();
      task.ExpenseReportGroup.ExpenseReports.Add(document);
      return task;
    }
    
    /// <summary>
    /// Создать авансовый отчет.
    /// </summary>
    /// <returns>Авансовый отчет.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public static IExpenseReport CreateNewExpenseReport()
    {
      return DirRX.ExpenseReports.ExpenseReports.Create();
    }

    /// <summary>
    /// Получить вид документа, созданный при инциализации.
    /// </summary>
    /// <param name="documentKindEntityGuid">ИД экземпляра, созданного при инициализации.</param>
    /// <returns>Вид документа.</returns>
    [Remote(IsPure = true), Public]
    public static Sungero.Docflow.IDocumentKind GetDocumentKind(Guid documentKindEntityGuid)
    {
      var externalLink = Sungero.Docflow.PublicFunctions.Module.GetExternalLink(Sungero.Docflow.Server.DocumentKind.ClassTypeGuid, documentKindEntityGuid);

      return Sungero.Docflow.DocumentKinds.GetAll().Where(x => x.Id == externalLink.EntityId).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить роль.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>Роль.</returns>
    [Remote(PackResultEntityEagerly = true, IsPure = true), Public]
    public static IRole GetRole(Guid roleGuid)
    {
      return Roles.GetAll(r => r.Sid == roleGuid).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить все данные для валидации подписания одним запросом.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Список структур с данными валидации.</returns>
    [Remote(IsPure = false)]
    public Structures.Module.BeforeSign ValidateBeforeSign(IOfficialDocument document)
    {
      var currentEmployee = Sungero.Company.Employees.Current;
      
      var errors = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalValidationErrors(document, false);
      
      var signatories =  Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetEmployeeSignatories(document);
      var canApprove = document.AccessRights.CanApprove() && signatories.Contains(currentEmployee.Id);

      return Structures.Module.BeforeSign.Create(errors, canApprove);
    }
    
    /// <summary>
    /// Сконвертировать последнюю версию документа в PDF
    /// </summary>
    [Public]
    public void ConvertLastVersionToPDF(IElectronicDocument document)
    {
      
      var version = document.LastVersion;
      
      var pdfConverter = new Sungero.AsposeExtensions.Converter();
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, version.BodyAssociatedApplication.Extension);
        }
        catch (Sungero.AsposeExtensions.PdfConvertException ex)
        {
          Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), ex.InnerException);
        }
      }
      
      // Если не сгенерировалась pdf - значит не поддерживается формат.
      if (pdfDocumentStream != null)
      {
        version.Body.Write(pdfDocumentStream);
        version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
        document.Save();
        pdfDocumentStream.Close();
      }
    }
    
    /// <summary>
    /// Получить сотрудников по Нашей орг.
    /// </summary>
    /// <param name="businessUnit">Наша орг.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote(IsPure = true)]
    public static List<Sungero.Company.IEmployee> GetEmployeesByBusinessUnit(Sungero.Company.IBusinessUnit businessUnit)
    {
      if (businessUnit != null)
        return Sungero.Company.Employees.GetAll(e => e.Department.BusinessUnit.Equals(businessUnit)).ToList();
      else
        return Sungero.Company.Employees.GetAll().ToList();
    }
    
    #region Экспорт документов по авансовым отчетам с ЭП

    /// <summary>
    /// Поиск документов по авансовым отчетам для экспорта с учетом фильтрации.
    /// </summary>
    /// <param name="filter">Фильтр.</param>
    /// <param name="withSupportingDocs">Добавить подтверждающие документы.</param>
    /// <returns>Список документов по авансовым отчетам.</returns>
    [Remote(IsPure=true), Public]
    public static List<IOfficialDocument> SearchExpenseReportDocsByRequisites(DirRX.ExpenseReports.Structures.Module.IExportDialogSearch filter, bool withSupportingDocs)
    {
      var resultDocsList = new List<IOfficialDocument>();
      
      var expenseReports = ExpenseReports.GetAll(d => d.HasVersions &&
                                                 d.ExpenseReportStatus == DirRX.ExpenseReports.ExpenseReport.ExpenseReportStatus.Approved);

      if (filter.Unit != null)
        expenseReports = expenseReports.Where(d => Equals(d.BusinessUnit, filter.Unit));
      
      if (filter.Employee != null)
        expenseReports = expenseReports.Where(d => Equals(d.Employee, filter.Employee));

      if (filter.From.HasValue)
        expenseReports = expenseReports.Where(q => q.RegistrationDate != null && q.RegistrationDate >= filter.From);
      
      if (filter.To.HasValue)
        expenseReports = expenseReports.Where(q => q.RegistrationDate != null && q.RegistrationDate <= filter.To);
      
      resultDocsList.AddRange(expenseReports);
      
      if (withSupportingDocs)
      {
        foreach (var expenseReport in expenseReports)
        {
          // Добавить подтверждающие документы из таблицы расходов
          
          foreach (var expense in expenseReport.Expenses)
          {
            if (expense.SupportingDoc != null)
              resultDocsList.Add(expense.SupportingDoc);
          }
          
          // Добавить прочие связанные документы (служебку)
          foreach (var relatedDoc in expenseReport.Relations.GetRelated(PublicConstants.Module.SimpleRelationName))
          {
            if (OfficialDocuments.Is(relatedDoc))
              resultDocsList.Add(OfficialDocuments.As(relatedDoc));
          }
        }
      }
      
      return resultDocsList;
    }
    
    /// <summary>
    /// Подготовить выбранные документы по авансовым отчетам для экспорта.
    /// </summary>
    /// <param name="filter">Параметры поиска.</param>
    /// <returns>Документы по авансовым отчетам.</returns>
    [Remote]
    public List<Structures.Module.ExportedDocument> PrepareExportExpenseReportDialogDocuments(DirRX.ExpenseReports.Structures.Module.IExportDialogSearch filter)
    {
      var result = new List<Structures.Module.ExportedDocument>();
      
      var docs = Functions.Module.SearchExpenseReportDocsByRequisites(filter, false);
      
      foreach (var document in docs)
      {
        var docName = CommonLibrary.FileUtils.NormalizeFileName(document.Name);
        docName = docName.Substring(0, Math.Min(docName.Length, 50))  + " (" + document.Id + ")";
        
        var docStructure = Structures.Module.ExportedDocument
          .Create(document.Id, false, string.Empty, Structures.Module.ExportedFolder
                  .Create(docName), docName);
        
        result.Add(docStructure);
        
        // Добавить подтверждающие документы из таблицы расходов
        var expenseReport = ExpenseReports.As(document);
        foreach (var expense in expenseReport.Expenses)
        {
          if (expense.SupportingDoc != null)
          {
            var supportingDocName = CommonLibrary.FileUtils.NormalizeFileName(expense.SupportingDoc.Name);
            var supportingDocStructure = Structures.Module.ExportedDocument
              .Create(expense.SupportingDoc.Id, false, string.Empty, Structures.Module.ExportedFolder
                      .Create(docName), supportingDocName);
            
            result.Add(supportingDocStructure);
          }
        }
        
        // Добавить прочие связанные документы (служебку)
        if (expenseReport.HasRelations)
        {
          foreach (var relatedDoc in expenseReport.Relations.GetRelated(PublicConstants.Module.SimpleRelationName))
          {
            var relatedDocName = CommonLibrary.FileUtils.NormalizeFileName(relatedDoc.Name);
            var relatedDocStructure = Structures.Module.ExportedDocument
              .Create(relatedDoc.Id, false, string.Empty, Structures.Module.ExportedFolder
                      .Create(docName), relatedDocName);
            
            result.Add(relatedDocStructure);
          }
        }
        
      }
      return result;
    }

    /// <summary>
    /// Создать ZIP архив со всеми выгружаемыми документами.
    /// </summary>
    /// <param name="zipModels">Список моделей.</param>
    /// <param name="exportResults">Выгружаемые документы.</param>
    /// <returns>ZIP архив.</returns>
    [Remote]
    public static IZip CreateZipFromZipModel(List<Structures.Module.ZipModel> zipModels, Structures.Module.ExportResults exportResults)
    {
      var zip = Sungero.Core.Zip.Create();
      foreach (var zipModel in zipModels)
      {
        if (zipModel.Body == null)
        {
          var document = Sungero.Docflow.OfficialDocuments.Get(zipModel.DocumentId.Value);
          var version = document.Versions.Where(x => x.Id == zipModel.VersionId).FirstOrDefault();
          if (zipModel.SignatureId != null)
          {
            
            var signature = Signatures.Get(version).Where(x => x.Id == zipModel.SignatureId).SingleOrDefault();
            zip.Add(signature, zipModel.FileName, zipModel.FolderRelativePath.ToArray());
            continue;
          }
          zip.Add(version.Body, zipModel.FileName, zipModel.FolderRelativePath.ToArray());
        }
        else
        {
          var internalZip = (Sungero.Domain.IInternalZip)zip;
          internalZip.Add(zipModel.Body, zipModel.FileName, "pdf", zipModel.FolderRelativePath.ToArray());
        }
      }

      // Создать лог-файл
      if (exportResults.ExportedDocuments.Any(d => d.IsFaulted))
      {
        var errorText = new List<string>();
        foreach (var errorDoc in exportResults.ExportedDocuments.Where(d => d.IsFaulted))
        {
          errorText.Add(Resources.ExportLogTemplateFormat(errorDoc.Id, errorDoc.Name, errorDoc.Error));
        }
        var errorAllText = String.Join(Environment.NewLine, errorText.ToArray());
        using (var logFileStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(errorAllText)))
        {
          // HACK Преобразовать тип к IInternalZip, чтобы можно было добавить массив байт в zip. Убрать после доработки платформы
          var internalZip = (Sungero.Domain.IInternalZip)zip;
          internalZip.Add(logFileStream.ToArray(), "Log", "log", (new List<string>()).ToArray());
        }
      }
      zip.Save(exportResults.RootFolder);
      return zip;
    }
    
    
    /// <summary>
    /// Подготовить тела документов и подписи для выгрузки.
    /// </summary>
    /// <param name="exportedDocs">Документы.</param>
    [Remote]
    public Structures.Module.ExportResults PrepareBodiesAndSigns(List<Structures.Module.ExportedDocument> exportedDocs)
    {
      var zipModels = new List<Structures.Module.ZipModel>();
      foreach (var exportedDoc in exportedDocs)
      {
        try
        {
          var doc = Sungero.Docflow.OfficialDocuments.Get(exportedDoc.Id);
          if (!doc.HasVersions)
          {
            exportedDoc.IsFaulted = true;
            exportedDoc.Error = Resources.ExportDialog_Error_NoVersion;
            continue;
          }
          
          var version = doc.LastVersion;
          var fileName = GetDocumentNameForExport(doc);
          var folder = exportedDoc.Folder;

          AddVersionBodyToZipModel(version, fileName, folder, doc.Id, zipModels);
          AddSignaturesToZipModel(version, fileName, folder, zipModels);
          GenerateDocWithSignatureMarks(version, fileName, folder, zipModels, exportedDoc);

          var operation = new Enumeration(Constants.ExpenseReport.ExportToFolder);
          doc.History.Write(operation, operation, string.Empty, version.Number);
        }
        catch (Exception ex)
        {
          Logger.Debug(ex.ToString());
          exportedDoc.Error = Resources.ExportDialog_Error_ClientFormat(ex.Message.TrimEnd('.'));
          exportedDoc.IsFaulted = true;
        }
      }
      var now = Sungero.Core.Calendar.UserNow;
      var tempFolderName = Resources.ExportDocumentFolderNameFormat(now.ToShortDateString() + " " + now.ToLongTimeString()).ToString();
      tempFolderName = CommonLibrary.FileUtils.NormalizeFileName(tempFolderName);
      
      return Structures.Module.ExportResults.Create(tempFolderName, exportedDocs, zipModels);
    }
    
    private static void AddVersionBodyToZipModel(Sungero.Content.IElectronicDocumentVersions version, string docName,
                                                 Structures.Module.ExportedFolder folder, int documentId,
                                                 List<Structures.Module.ZipModel> zipModels)
    {
      var body = version.Body;
      var extension = version.BodyAssociatedApplication.Extension;
      var fileName = docName + "." + extension;

      var zipModel = Structures.Module.ZipModel.Create();
      zipModel.DocumentId = documentId;
      zipModel.VersionId = version.Id;
      zipModel.FileName = fileName;
      zipModel.FolderRelativePath = (new string[] { folder.FolderName }).ToList();
      zipModel.Size = body.Size;
      zipModels.Add(zipModel);
    }
    
    /// <summary>
    /// Пополучить имя документа для экспорта с усеченной длиной и ИД в конце.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Имя.</returns>
    [Public]
    public static string GetDocumentNameForExport(Sungero.Docflow.IOfficialDocument document)
    {
      var name = document.Name.Substring(0, Math.Min(document.Name.Length, 100)) + document.Id;
      return CommonLibrary.FileUtils.NormalizeFileName(name);
    }
    
    /// <summary>
    /// Добавить подписи, которые созданы с помощью сертификата, в выгрузку.
    /// </summary>
    /// <param name="version">Версия документа.</param>
    /// <param name="folder">Папка.</param>
    /// <param name="zipModels">Список моделей.</param>
    private static void AddSignaturesToZipModel(Sungero.Content.IElectronicDocumentVersions version,
                                                string fileName, Structures.Module.ExportedFolder folder,
                                                List<Structures.Module.ZipModel> zipModels)
    {
      var signatures = Signatures.Get(version).ToList();
      if (signatures.Count > 0)
      {
        var signNumber = 0;
        foreach (var sign in signatures)
        {
          if (sign.SignCertificate != null)
          {
            var signData = ((Sungero.Domain.Shared.IInternalSignature)sign).GetDataSignature();
            var signFullFileName = fileName + "_" + signNumber;
            
            var zipModel = Structures.Module.ZipModel.Create();
            zipModel.DocumentId = version.ElectronicDocument.Id;
            zipModel.VersionId = version.Id;
            zipModel.FileName = signFullFileName;
            zipModel.FolderRelativePath = (new string[] { folder.FolderName }).ToList();
            zipModel.SignatureId = sign.Id;
            zipModel.Size = signData.LongLength;
            zipModels.Add(zipModel);
            
            signNumber++;
          }
        }
      }
    }
    
    /// <summary>
    /// Сгенерировать preview документа с отметками об ЭП.
    /// </summary>
    /// <param name="version">Версия документа.</param>
    /// <param name="docName">Имя файла документа.</param>
    /// <param name="folder">Папка.</param>
    /// <param name="zipModels">Список моделей.</param>
    /// <param name="exportedDoc">Описание документа.</param>
    public static void GenerateDocWithSignatureMarks(Sungero.Content.IElectronicDocumentVersions version,
                                                     string docName,
                                                     Structures.Module.ExportedFolder folder,
                                                     List<Structures.Module.ZipModel> zipModels,
                                                     Structures.Module.ExportedDocument exportedDoc)
    {
      var signatures = Signatures.Get(version).ToList();
      if (signatures.Count > 0)
      {
        var previewFullFileName = docName + "_Preview.pdf";
        System.IO.Stream pdfDocumentStream = null;
        using (var inputStream = new System.IO.MemoryStream())
        {
          version.Body.Read().CopyTo(inputStream);
          try
          {
            var pdfConverter = new Sungero.AsposeExtensions.Converter();
            if (version.AssociatedApplication.Extension.ToLower() != "pdf")
            {
              pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, version.AssociatedApplication.Extension);
            }
            else
              pdfDocumentStream = inputStream;
            
            var htmlStampString = GetSignatureMarkAsHtml(signatures);
            
            pdfDocumentStream = pdfConverter.AddSignatureMark(pdfDocumentStream, "pdf", htmlStampString, Sungero.Docflow.Resources.SignatureMarkAnchorSymbol, 1);
            var previewStream = new System.IO.MemoryStream();
            pdfDocumentStream.CopyTo(previewStream);
            
            var zipModel = Structures.Module.ZipModel.Create();
            zipModel.FileName = previewFullFileName;
            zipModel.FolderRelativePath = (new string[] { folder.FolderName }).ToList();
            zipModel.Size = previewStream.ToArray().Count();
            zipModel.Body = previewStream.ToArray();

            zipModels.Add(zipModel);
            
            pdfDocumentStream.Close();
          }
          catch (Exception e)
          {
            if (e is Sungero.AsposeExtensions.PdfConvertException)
              Logger.Error(Resources.CreatePreviewErrorFormat(docName), e.InnerException);
            else
              Logger.Error(string.Format("{0} {1}", Resources.CreatePreviewErrorFormat(docName), e.Message));
            
            exportedDoc.IsFaulted = true;
            exportedDoc.Error = Resources.CreatePreviewErrorFormat(docName);
          }
        }
        
      }
    }
    
    /// <summary>
    /// Получить штамп с информацией о подписях.
    /// </summary>
    /// <param name="signatureList">Подписи.</param>
    /// <returns>Штамп с информацией о подписях в виде html.</returns>
    [Public]
    public static string GetSignatureMarkAsHtml(List<Sungero.Domain.Shared.ISignature> signatureList)
    {
      if (signatureList == null)
        return string.Empty;
      
      string html = Resources.HtmlStampTemplate;
      
      var stampList = new List<string>();
      foreach (var signature in signatureList)
      {
        // В случае квалифицированной ЭП информацию для отметки брать из атрибутов субъекта сертификата.
        var certificate = signature.SignCertificate;
        if (certificate != null)
        {
          string templateForCertSignature = Resources.HtmlStampTemplateForCertSignature;

          var certificateSubject = Sungero.Docflow.PublicFunctions.Module.GetCertificateSubject(signature);
          var signatoryFullName = string.Format("{0} {1}", certificateSubject.Surname, certificateSubject.GivenName).Trim();
          if (string.IsNullOrEmpty(signatoryFullName))
            signatoryFullName = certificateSubject.CounterpartyName;
          
          templateForCertSignature = templateForCertSignature.Replace("{SignatoryFullName}", signatoryFullName);
          templateForCertSignature = templateForCertSignature.Replace("{Thumbprint}", certificate.Thumbprint.ToLower());
          
          var validity = string.Format("{0} {1} {2} {3}",
                                       Sungero.Company.Resources.From,
                                       certificate.NotBefore.Value.ToShortDateString(),
                                       Sungero.Company.Resources.To,
                                       certificate.NotAfter.Value.ToShortDateString());
          templateForCertSignature = templateForCertSignature.Replace("{Validity}", validity);
          
          stampList.Add(templateForCertSignature);
        }
        // В случае простой ЭП информацию для отметки брать из атрибутов подписи.
        else
        {
          string templateForSimpleSignature = Resources.HtmlStampTemplateForSimpleSignature;
          
          var signatoryFullName = signature.SignatoryFullName;
          templateForSimpleSignature = templateForSimpleSignature.Replace("{SignatoryFullName}", signatoryFullName);
          var signatoryId = signature.Signatory.Id;
          templateForSimpleSignature = templateForSimpleSignature.Replace("{SignatoryId}", signatoryId.ToString());

          stampList.Add(templateForSimpleSignature);
        }
      }
      html = html.Replace("{StampList}", string.Join("\r\n", stampList.ToArray()));
      return html;
    }
    
    #endregion
    
  }
}