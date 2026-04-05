using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Finalspace.Onigiri.MVVM;

public abstract class ViewModelBase : BindableBase, INotifyDataErrorInfo, IViewModelLifeTime, ISupportServices
{
    private ConcurrentDictionary<string, ImmutableArray<string>> _errors;

    public IServiceContainer ServiceContainer => _serviceContainer;
    private readonly ServiceContainer _serviceContainer = new ServiceContainer();

    private ConcurrentDictionary<string, ImmutableArray<string>> GetOrCreateErrors()
    {
        if (_errors is not null) return _errors;
        Interlocked.CompareExchange(ref _errors, new ConcurrentDictionary<string, ImmutableArray<string>>(), null);
        return _errors;
    }

    public bool HasErrors => _errors is not null && _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    
    public bool IsLoaded { get => _isLoaded; set => SetProperty(ref _isLoaded, value); }

    private bool _isLoaded = false;

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
