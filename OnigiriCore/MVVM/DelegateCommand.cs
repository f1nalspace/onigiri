using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace Finalspace.Onigiri.MVVM;

public class DelegateCommand<T> : ICommand, IDisposable
{
    private readonly RelayCommand<T> _wrapper;

    public event EventHandler CanExecuteChanged;
    
    private bool _isDisposed = false;

    public DelegateCommand(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _wrapper = new RelayCommand<T>(action);
        _wrapper.CanExecuteChanged += WrapperOnCanExecuteChanged;
    }
    
    public DelegateCommand(Action<T> action, Predicate<T> canExecute)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(canExecute);
        _wrapper = new RelayCommand<T>(action, canExecute);
        _wrapper.CanExecuteChanged += WrapperOnCanExecuteChanged;
    }
    
    private void WrapperOnCanExecuteChanged(object sender, EventArgs e)
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public void RaiseCanExecuteChanged() => _wrapper.NotifyCanExecuteChanged();
    
    public bool CanExecute(object parameter) => _wrapper.CanExecute(parameter);
    public bool CanExecute(T parameter) => _wrapper.CanExecute(parameter);
    
    public void Execute(object parameter) => _wrapper.Execute(parameter);
    public void Execute(T parameter) => _wrapper.Execute(parameter);
    
    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        _wrapper.CanExecuteChanged -= WrapperOnCanExecuteChanged;
    }
}

public class DelegateCommand : DelegateCommand<object>
{
    public DelegateCommand(Action action) : base(new Action<object>(o => action()), o => true)
    {
    }
    
    public DelegateCommand(Action action, Func<bool> canExecute) : base(new Action<object>(o => action()), o => canExecute())
    {
    }
}


