using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JimBobBennett.JimLib.Extensions;
using JimBobBennett.JimLib.Mvvm;

namespace JimBobBennett.RestAndRelaxForPlex.PlexObjects
{
    public abstract class PlexObjectBase<T> : NotificationObject
    {
        protected abstract bool OnUpdateFrom(T newValue, List<string> updatedPropertyNames);

        public bool UpdateFrom(T newValue)
        {
            var updatedPropertyNames = new List<string>();
            var updated = OnUpdateFrom(newValue, updatedPropertyNames);

            if (updated)
            {
                foreach (var updatedPropertyName in updatedPropertyNames)
                    RaisePropertyChanged(updatedPropertyName);
            }

            return updated;
        }

        public abstract string Key { get; }

        protected bool UpdateValue<TValue>(Expression<Func<TValue>> propertyExpression, T newValue,
            List<string> updatedPropertyNames)
        {
            var propertyInfo = this.ExtractPropertyInfo(propertyExpression);

            var value = propertyInfo.GetValue(newValue);
            var thisValue = propertyInfo.GetValue(this);

            if (Equals(thisValue, value))
                return false;

            propertyInfo.SetValue(this, value);
            updatedPropertyNames.Add(propertyInfo.Name);

            return true;
        }
    }
}
