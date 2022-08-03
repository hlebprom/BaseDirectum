using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.BusinessTrips.BusinessTrip;
using Sungero.Content;
using Sungero.Company;

namespace DirRX.BusinessTrips.Server
{
  partial class BusinessTripFunctions
  {
    /// <summary>
    /// Получить список документов, относящихся к командировке.
    /// </summary>
    /// <returns>Список документов.</returns>
    [Remote(IsPure = true)]
    public virtual List<IElectronicDocument> GetBusinessTripDocuments()
    {
      var result = new List<IElectronicDocument> ();
      result.AddRange(BusinessTripOrders.GetAll(d => Equals(_obj, d.BusinessTrip)));
      var task = GetBusinessTripApprovalTask();
      
      var docs = task.OtherGroup.All.Where(t => ElectronicDocuments.Is(t));
      if (docs.Any())
        result.AddRange(docs.Cast<IElectronicDocument>());
      
      if (_obj.ExpenseReport != null)
        result.Add(_obj.ExpenseReport);
      
      return result;
    }
    
    /// <summary>
    /// Получить задачи по командировке.
    /// </summary>
    /// <returns>Задачи по командировке.</returns>
    [Remote(IsPure = true)]
    public virtual List<Sungero.Workflow.ITask> GetBusinessTripTasks()
    {
      var result = new List<Sungero.Workflow.ITask> ();
      result.Add(GetBusinessTripApprovalTask());
      result.AddRange(BusinessTripRecallTasks.GetAll(t => Equals(t.BusinessTrip, _obj)));
      result.AddRange(BusinessTripCancelTasks.GetAll(t => Equals(t.BusinessTrip, _obj)));
      
      return result;
    }
    
    /// <summary>
    /// Получить задачу на согласование командировки.
    /// </summary>
    /// <returns>Задача на согласование командировки.</returns>
    [Remote(IsPure = true)]
    public virtual IBusinessTripApprovalTask GetBusinessTripApprovalTask()
    {
      return BusinessTripApprovalTasks.GetAll(t => t.BusinessTripId == _obj.Id).FirstOrDefault();
    }
  }
}