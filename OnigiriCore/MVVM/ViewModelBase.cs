using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Finalspace.Onigiri.MVVM;

public abstract class ViewModelBase : ObservableObject, INotifyDataErrorInfo, IViewModelLifeTime, IIOCQuery
{
    private ConcurrentDictionary<string, ImmutableArray<string>> _errors;

    private ConcurrentDictionary<string, ImmutableArray<string>> GetOrCreateErrors()
    {
        if (_errors is not null) return _errors;
        Interlocked.CompareExchange(ref _errors, new ConcurrentDictionary<string, ImmutableArray<string>>(), null);
        return _errors;
    }

    public bool HasErrors => _errors is not null && _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

    public IOCContainer IOC
    {
        get
        {
            if (_ioc is not null) return _ioc;
            Interlocked.CompareExchange(ref _ioc, new IOCContainer(), null);
            return _ioc;
        }
    }

    public bool IsLoaded { get => _isLoaded; set => SetProperty(ref _isLoaded, value); }

    private bool _isLoaded = false;

    private IOCContainer _ioc;

    public T QueryService<T>()
    {
        if (_ioc is not null)
        {
            T result = _ioc.QueryService<T>();
            if (result is not null) return result;
        }
        return MVVM.IOC.Default.QueryService<T>();
    }

    public object QueryService(Type type) => _ioc?.QueryService(type) ?? MVVM.IOC.Default.QueryService(type);

    public virtual void LoadView(IView view) { }
    public virtual void UnloadView(IView view) { }

    void IViewModelLifeTime.Loaded(IView view)
    {
        if (IsLoaded)
            return;
        LoadView(view);
        IsLoaded = true;
    }

    void IViewModelLifeTime.Unloaded(IView view)
    {
        if (!IsLoaded)
            return;
        UnloadView(view);
        IsLoaded = false;
    }
    
    public IEnumerable GetErrors(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName) || _errors is null || !_errors.TryGetValue(propertyName, out ImmutableArray<string> errors))
            return Enumerable.Empty<string>();
        return errors;
    }

    protected void ClearErrors(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        _errors?.TryRemove(propertyName, out _);
    }

    protected void SetErrors(string propertyName, params string[] messages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ConcurrentDictionary<string, ImmutableArray<string>> errors = GetOrCreateErrors();
        errors.AddOrUpdate(propertyName, messages.ToImmutableArray(), (k, o) => messages.ToImmutableArray());
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    protected void AddErrors(string propertyName, params string[] messages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ConcurrentDictionary<string, ImmutableArray<string>> errors = GetOrCreateErrors();
        errors.AddOrUpdate(propertyName, messages.ToImmutableArray(), (k, o) => o.AddRange(messages));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
