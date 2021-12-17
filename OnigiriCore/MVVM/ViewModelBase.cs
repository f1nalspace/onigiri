using System;
using System.Windows.Threading;

namespace Finalspace.Onigiri.MVVM
{
    public class ViewModelBase : BindableBase
    {
        private Dispatcher DefaultDispatcher;

        public T GetService<T>() where T : class
        {
            return ServiceContainer.Instance.Get<T>();
        }

        protected void UIInvoke(Action a, bool async = false)
        {
            if (async)
                DefaultDispatcher.BeginInvoke(a);
            else
                DefaultDispatcher.Invoke(a);
        }

        public ViewModelBase()
        {
            DefaultDispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
