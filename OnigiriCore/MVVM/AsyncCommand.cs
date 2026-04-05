using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Finalspace.Onigiri.MVVM;

public class AsyncCommand : AsyncCommand<object> {

    public AsyncCommand(Func<object, Task> execute) : base(execute)
    {
    }
    
    public AsyncCommand(Func<object, Task> execute, Predicate<object> canExecute) : base(execute, canExecute)
    {
    }
}

public class AsyncCommand<T> : ICommand, IDisposable
{
    private readonly AsyncRelayCommand<T> _wrapper;
    
    public event EventHandler CanExecuteChanged;
    
    private bool _isDisposed = false;

    public AsyncCommand(Func<T, Task> execute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _wrapper = new AsyncRelayCommand<T>(execute);
        _wrapper.CanExecuteChanged += WrapperOnCanExecuteChanged;
    }
    
    public AsyncCommand(Func<T, Task> execute, Predicate<T> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _wrapper = new AsyncRelayCommand<T>(execute, canExecute);
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
