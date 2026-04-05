using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Finalspace.Onigiri.MVVM;

public abstract class BindableBase : ObservableObject
{
    private readonly ConcurrentDictionary<string, object> _propertyBag = new();
    
    protected T GetValue<T>([CallerMemberName] string propertyName = null)
    {
        if (_propertyBag.TryGetValue(propertyName!, out object value))
            return (T)value;
        return default;
    }

    protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
    {
        if (_propertyBag.TryGetValue(propertyName!, out object existing) && EqualityComparer<T>.Default.Equals((T)existing, value))
            return;
        OnPropertyChanging(propertyName);
        _propertyBag[propertyName!] = value;
        OnPropertyChanged(propertyName);
    }

    protected void SetValue<T>(T value, Action callback, [CallerMemberName] string propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(callback);
        if (_propertyBag.TryGetValue(propertyName!, out object existing) && EqualityComparer<T>.Default.Equals((T)existing, value))
            return;
        OnPropertyChanging(propertyName);
        _propertyBag[propertyName!] = value;
        OnPropertyChanged(propertyName);
        callback();
    }

    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        => OnPropertyChanged(propertyName!);

    protected void RaisePropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        var memberExpr = (System.Linq.Expressions.MemberExpression)propertyExpression.Body;
        OnPropertyChanged(memberExpr.Member.Name);
    }

    protected void RaisePropertiesChanged(params string[] propertyNames)
    {
        ArgumentNullException.ThrowIfNull(propertyNames);
        foreach (string name in propertyNames)
            RaisePropertyChanged(name);
    }
    
    protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, Action<T> callback, [CallerMemberName] string propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(callback);

        T oldValue = field;

        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return false;

        OnPropertyChanging(propertyName);
        field = newValue;
        callback(newValue);
        OnPropertyChanged(propertyName);

        return true;
    }

    protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, Action callback, [CallerMemberName] string propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(callback);

        T oldValue = field;

        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return false;

        OnPropertyChanging(propertyName);
        field = newValue;
        callback();
        OnPropertyChanged(propertyName);

        return true;
    }
}
