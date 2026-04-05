using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Finalspace.Onigiri.MVVM;

/// <summary>
/// Lightweight base class for ViewModels that only need property change notification
/// and change tracking. Does not include validation errors, IOC container, or view lifetime.
/// </summary>
public abstract class BindableBase : ObservableObject
{
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
