using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow.SignatureSetting;

namespace Sungero.Docflow
{

  partial class SignatureSettingSharedHandlers
  {

    public virtual void BusinessUnitsChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      Functions.SignatureSetting.ChangePropertiesAccess(_obj);
      _obj.Certificate = null;
    }

    public virtual void DocumentKindsChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      var categories = Functions.SignatureSetting.GetPossibleCashedCategories(_obj);
      var objCategories = _obj.Categories.Select(c => c.Category).Where(c => categories.Contains(c)).ToList();
      
      if (objCategories.Count < _obj.Categories.Count())
      {
        Docflow.PublicFunctions.Module.TryToShowNotifyMessage(SignatureSettings.Resources.IncompatibleCategoriesExcluded);
        _obj.Categories.Clear();
        foreach (var category in objCategories)
          _obj.Categories.AddNew().Category = category;
      }
      
      _obj.State.Properties.Categories.IsEnabled = categories.Any();
    }

    public virtual void DocumentFlowChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null && e.NewValue != SignatureSetting.DocumentFlow.All)
      {
        var suitableDocumentKinds = _obj.DocumentKinds.Where(dk => dk.DocumentKind.DocumentFlow == e.NewValue).Select(k => k.DocumentKind).ToList();

        if (suitableDocumentKinds.Count < _obj.DocumentKinds.Count())
        {
          Functions.Module.TryToShowNotifyMessage(SignatureSettings.Resources.IncompatibleDocumentKindsExcluded);
          _obj.DocumentKinds.Clear();
          foreach (var documentKind in suitableDocumentKinds)
            _obj.DocumentKinds.AddNew().DocumentKind = documentKind;
        }
      }
      
      var categories = Functions.SignatureSetting.GetPossibleCashedCategories(_obj);
      _obj.State.Properties.Categories.IsEnabled = categories.Any();
      
      if (!_obj.State.Properties.Categories.IsEnabled && _obj.Categories.Any())
      {
        Docflow.PublicFunctions.Module.TryToShowNotifyMessage(SignatureSettings.Resources.IncompatibleCategoriesExcluded);
        _obj.Categories.Clear();
      }
    }

    public virtual void DocumentChanged(Sungero.Docflow.Shared.SignatureSettingDocumentChangedEventArgs e)
    {
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue) && _obj.Reason == Docflow.SignatureSetting.Reason.PowerOfAttorney)
      {
        if (PowerOfAttorneys.Is(e.NewValue))
        {
          var powerOfAttorney = PowerOfAttorneys.As(e.NewValue);
          _obj.Recipient = powerOfAttorney.IssuedTo;
          _obj.ValidFrom = powerOfAttorney.RegistrationDate;
          _obj.ValidTill = powerOfAttorney.ValidTill;
          _obj.BusinessUnits.Clear();
          var newBusinessUnit = _obj.BusinessUnits.AddNew();
          newBusinessUnit.BusinessUnit = powerOfAttorney.BusinessUnit;
        }
        else
        {
          _obj.Recipient = null;
          _obj.ValidTill = null;
          _obj.BusinessUnits.Clear();
        }
      }
    }

    public virtual void RecipientChanged(Sungero.Docflow.Shared.SignatureSettingRecipientChangedEventArgs e)
    {
      // При изменении подписывающего почистить документ.
      if (_obj.Reason == Docflow.SignatureSetting.Reason.PowerOfAttorney &&
          _obj.Document != null && Docflow.PowerOfAttorneys.Is(_obj.Document) && !Equals(e.NewValue, PowerOfAttorneys.As(_obj.Document).IssuedTo))
      {
        _obj.Document = null;
        _obj.ValidFrom = null;
        _obj.ValidTill = null;
      }
      
      if (!Equals(e.NewValue, e.OldValue))
        _obj.Certificate = null;
      Functions.SignatureSetting.ChangePropertiesAccess(_obj);
    }

    public virtual void ReasonChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (!Equals(e.NewValue, e.OldValue))
      {
        _obj.Document = null;
        _obj.DocumentInfo = null;
      }
      
    }

    public virtual void AmountChanged(Sungero.Domain.Shared.DoublePropertyChangedEventArgs e)
    {
      if (e.NewValue == null)
        return;
      
      // Подставить по умолчанию валюту рубль.
      if (_obj.Currency == null)
      {
        var defaultCurrency = Commons.PublicFunctions.Currency.Remote.GetDefaultCurrency();
        if (defaultCurrency != null)
          _obj.Currency = defaultCurrency;
      }
    }

    public virtual void LimitChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      // При смене типа ограничения очистить поля с суммой и валютой.
      if (e.NewValue != e.OldValue)
      {
        _obj.Amount = null;
        _obj.Currency = null;
      }
      Functions.SignatureSetting.ChangePropertiesAccess(_obj);
      
    }

  }
}