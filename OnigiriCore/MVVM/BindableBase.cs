using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Finalspace.Onigiri.MVVM
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, object> _propertyValueStorage = new Dictionary<string, object>();

        private string GetPropertyName(LambdaExpression lambdaExpression)
        {
            MemberExpression memberExpression;
            if (lambdaExpression.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = lambdaExpression.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
                memberExpression = lambdaExpression.Body as MemberExpression;
            return memberExpression.Member.Name;
        }

        protected void SetValue<T>(Expression<Func<T>> property, T value, Action changedHandler = null)
        {
            LambdaExpression lambdaExpression = property as LambdaExpression;
            if (lambdaExpression == null)
                throw new ArgumentException("Invalid lambda expression", "Lambda expression return value can't be null");
            string propertyName = GetPropertyName(lambdaExpression);
            T storedValue = GetValue<T>(propertyName);
            if (object.Equals(storedValue, value)) return;
            _propertyValueStorage[propertyName] = value;
            RaisePropertyChanged(propertyName);
            changedHandler?.Invoke();
        }

        protected T GetValue<T>(Expression<Func<T>> property)
        {
            LambdaExpression lambdaExpression = property as LambdaExpression;
            if (lambdaExpression == null)
                throw new ArgumentException("Invalid lambda expression", "Lambda expression return value can't be null");
            string propertyName = GetPropertyName(lambdaExpression);
            return GetValue<T>(propertyName);
        }

        private T GetValue<T>(string propertyName)
        {
            object value;
            if (_propertyValueStorage.TryGetValue(propertyName, out value))
                return (T)value;
            return default(T);

        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> lambdaExpression)
        {
            string propertyName = GetPropertyName(lambdaExpression);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
