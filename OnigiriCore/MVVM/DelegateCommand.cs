using System;
using System.Windows.Input;

namespace Finalspace.Onigiri.MVVM
{
    public class BasicDelegateCommand : DelegateCommand<object>
    {
        public BasicDelegateCommand(Action<object> execute, Predicate<object> canExecute = null) : base(execute, canExecute)
        {
        }
    }

    public class DelegateCommand<T> : ICommand where T : class
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            T value = parameter as T;
            return _canExecute(value);
        }

        public void Execute(object parameter)
        {
            T value = parameter as T;
            _execute(value);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                EventArgs args = new EventArgs();
                CanExecuteChanged(this, args);
            }
        }
    }
}
